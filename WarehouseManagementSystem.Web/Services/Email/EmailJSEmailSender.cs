using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace WarehouseManagementSystem.Web.Services.Email
{
    public class EmailJSEmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly EmailJsOptions _options;

        public EmailJSEmailSender(HttpClient httpClient, IOptions<EmailJsOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_options.ServiceId) ||
                string.IsNullOrWhiteSpace(_options.TemplateId) ||
                string.IsNullOrWhiteSpace(_options.PublicKey))
            {
                throw new InvalidOperationException("EmailJS settings are missing ServiceId, TemplateId or PublicKey.");
            }

            var request = new Dictionary<string, object?>
            {
                ["service_id"] = _options.ServiceId,
                ["template_id"] = _options.TemplateId,
                ["user_id"] = _options.PublicKey,
                ["template_params"] = new
                {
                    to_email = email,
                    subject,
                    message = htmlMessage
                }
            };

            if (!string.IsNullOrWhiteSpace(_options.PrivateKey))
            {
                request["accessToken"] = _options.PrivateKey;
            }

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.emailjs.com/api/v1.0/email/send",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                throw new InvalidOperationException(
                    $"EmailJS returned {(int)response.StatusCode} {response.ReasonPhrase}: {responseBody}");
            }
        }
    }
}
