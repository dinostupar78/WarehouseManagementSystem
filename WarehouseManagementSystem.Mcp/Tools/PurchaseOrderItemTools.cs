using ModelContextProtocol.Server;
using System.Net.Http.Json;
using static WarehouseManagementSystem.Mcp.Tools.InventoryTools;

namespace WarehouseManagementSystem.Mcp.Tools
{
    [McpServerToolType]
    public class PurchaseOrderItemTools
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PurchaseOrderItemTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool(Name = "list_purchase_order_items")]
        public async Task<IEnumerable<PurchaseOrderItemMcpDto>?> ListPurchaseOrderItems()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<PurchaseOrderItemMcpDto>>("api/purchase-order-items");
        }

        [McpServerTool(Name = "get_purchase_order_item_by_id")]
        public async Task<PurchaseOrderItemMcpDto?> GetPurchaseOrderItemById(int id)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<PurchaseOrderItemMcpDto>($"api/purchase-order-items/{id}");
        }

        [McpServerTool(Name = "search_purchase_order_items")]
        public async Task<IEnumerable<PurchaseOrderItemMcpDto>?> SearchPurchaseOrderItems(string query)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<PurchaseOrderItemMcpDto>>(
                $"api/purchase-order-items?query={Uri.EscapeDataString(query)}");
        }

        [McpServerTool(Name = "get_high_value_purchase_order_items")]
        public async Task<IEnumerable<PurchaseOrderItemMcpDto>?> GetHighValuePurchaseOrderItems(decimal minimumSubtotal)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            var items = await client.GetFromJsonAsync<List<PurchaseOrderItemMcpDto>>("api/purchase-order-items");

            return items?
                .Where(i => i.Subtotal >= minimumSubtotal)
                .OrderByDescending(i => i.Subtotal)
                .ToList();
        }

        public class PurchaseOrderItemMcpDto
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Subtotal { get; set; }

            public PurchaseOrderSummaryMcpDto? PurchaseOrder { get; set; }
            public ProductSummaryMcpDto? Product { get; set; }
        }

        public class PurchaseOrderSummaryMcpDto
        {
            public int Id { get; set; }
            public int OrderNumber { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}
