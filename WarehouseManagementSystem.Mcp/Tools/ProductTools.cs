using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace WarehouseManagementSystem.Mcp.Tools
{
    [McpServerToolType]
    public class ProductTools
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool]
        [Description("Lists all products from the warehouse management system.")]
        public async Task<object?> ListProducts()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<object>("api/products");
        }

        [McpServerTool]
        [Description("Gets one product by its id.")]
        public async Task<object?> GetProductById(int id)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<object>($"api/products/{id}");
        }

        [McpServerTool]
        [Description("Searches products by name, description, price, weight or category.")]
        public async Task<object?> SearchProducts(string query)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<object>($"api/products?query={Uri.EscapeDataString(query)}");
        }

        [McpServerTool]
        [Description("Returns products with unit price greater than the selected minimum price.")]
        public async Task<object?> GetExpensiveProducts(decimal minimumPrice)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            var products = await client.GetFromJsonAsync<List<ProductMcpDto>>("api/products");

            return products?
                .Where(p => p.UnitPrice >= minimumPrice)
                .OrderByDescending(p => p.UnitPrice)
                .ToList();
        }

        public class ProductMcpDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Weight { get; set; }
            public DateTime ReceivedAt { get; set; }
        }

    }
}
