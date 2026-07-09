namespace WarehouseManagementSystem.Web.Services.AI
{
    public class GroqOptions
    {
        public string ApiKey { get; set; } 
        public string Model { get; set; } = "llama-3.3-70b-versatile";
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
    }
}
