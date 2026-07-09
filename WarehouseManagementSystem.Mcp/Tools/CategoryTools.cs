using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace WarehouseManagementSystem.Mcp.Tools
{
    [McpServerToolType]
    public class CategoryTools
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CategoryTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool(Name = "list_categories")]
        public async Task<IEnumerable<CategoryMcpDto>?> ListCategories()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<CategoryMcpDto>>("api/categories");
        }

        [McpServerTool(Name = "get_category_by_id")]
        public async Task<CategoryMcpDto?> GetCategoryById(int id)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<CategoryMcpDto>($"api/categories/{id}");
        }

        [McpServerTool(Name = "search_categories")]
        public async Task<IEnumerable<CategoryMcpDto>?> SearchCategories(string query)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<CategoryMcpDto>>(
                $"api/categories?query={Uri.EscapeDataString(query)}");
        }

        [McpServerTool(Name = "get_category_overview")]
        public async Task<CategoryOverviewMcpDto?> GetCategoryOverview()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");
            var categories = await client.GetFromJsonAsync<List<CategoryMcpDto>>("api/categories");

            if (categories == null)
            {
                return null;
            }

            return new CategoryOverviewMcpDto
            {
                TotalCategories = categories.Count,
                Categories = categories
                    .OrderBy(c => c.Name)
                    .ToList()
            };
        }

        public class CategoryMcpDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
        }

        public class CategoryOverviewMcpDto
        {
            public int TotalCategories { get; set; }
            public List<CategoryMcpDto> Categories { get; set; } = new();
        }

    }
}
