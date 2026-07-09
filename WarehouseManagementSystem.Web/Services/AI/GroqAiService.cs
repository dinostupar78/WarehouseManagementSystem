using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WarehouseManagementSystem.DAL.Data;

namespace WarehouseManagementSystem.Web.Services.AI
{
    public class GroqAiService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqOptions _options;
        private readonly AiPromptProvider _promptProvider;
        private readonly ILogger<GroqAiService> _logger;

        public GroqAiService(HttpClient httpClient, IOptions<GroqOptions> options, AiPromptProvider promptProvider, ILogger<GroqAiService> logger)
        {

            _httpClient = httpClient;
            _options = options.Value;
            _promptProvider = promptProvider;
            _logger = logger;
        }

        public async Task<string?> SuggestAsync(string entity, string prompt)
        {
            var systemPrompt = _promptProvider.GetSystemPrompt(entity);

            var requestBody = new
            {
                model = _options.Model,
                temperature = 0.1,
                messages = new[]
                {
            new
            {
                role = "system",
                content = systemPrompt
            },
            new
            {
                role = "user",
                content = prompt
            }
        },
                response_format = new
                {
                    type = "json_object"
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Groq AI suggestion failed for {Entity}. Status: {StatusCode}. Response: {Response}",
                    entity,
                    response.StatusCode,
                    responseText);

                return null;
            }

            var content = ExtractAssistantContent(responseText);

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Groq AI returned empty content for {Entity}", entity);
                return null;
            }

            return content;
        }

        private static string? ExtractAssistantContent(string responseText)
        {
            using var document = JsonDocument.Parse(responseText);

            return document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
    }
}
