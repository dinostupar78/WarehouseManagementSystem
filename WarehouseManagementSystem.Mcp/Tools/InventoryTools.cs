using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace WarehouseManagementSystem.Mcp.Tools
{
    [McpServerToolType]
    public class InventoryTools
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public InventoryTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool(Name = "list_inventory")]
        public async Task<IEnumerable<InventoryMcpDto>?> ListInventory()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<InventoryMcpDto>>("api/inventories");
        }

        [McpServerTool(Name = "get_inventory_by_id")]
        public async Task<InventoryMcpDto?> GetInventoryById(int id)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<InventoryMcpDto>($"api/inventories/{id}");
        }

        [McpServerTool(Name = "search_inventory")]
        public async Task<IEnumerable<InventoryMcpDto>?> SearchInventory(string query)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<InventoryMcpDto>>(
                $"api/inventories?query={Uri.EscapeDataString(query)}");
        }

        [McpServerTool(Name = "get_low_stock_inventory")]
        public async Task<IEnumerable<InventoryMcpDto>?> GetLowStockInventory(int threshold = 10)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            var inventory = await client.GetFromJsonAsync<List<InventoryMcpDto>>("api/inventories");

            return inventory?
                .Where(i => i.Quantity <= threshold)
                .OrderBy(i => i.Quantity)
                .ToList();
        }

        public class InventoryMcpDto
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
            public DateTime LastUpdated { get; set; }
            public ProductSummaryMcpDto? Product { get; set; }
            public LocationSummaryMcpDto? Location { get; set; }
        }

        public class ProductSummaryMcpDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class LocationSummaryMcpDto
        {
            public int Id { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Zone { get; set; } = string.Empty;
        }
    }
}
