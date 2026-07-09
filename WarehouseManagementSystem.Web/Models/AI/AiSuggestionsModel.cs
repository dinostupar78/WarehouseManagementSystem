namespace WarehouseManagementSystem.Web.Models.AI
{
    public class AiSuggestionsModel
    {
        public class AiSuggestionRequest
        {
            public string Entity { get; set; } 
            public string Prompt { get; set; } 
        }

        public class AiSuggestionResponse
        {
            public bool Success { get; set; }
            public string Entity { get; set; } 
            public object? Data { get; set; }
            public string? Message { get; set; }
        }
    }
}
