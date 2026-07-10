using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace WarehouseManagementSystem.Mcp.Tools
{
    [McpServerToolType]
    public class PurchaseOrderTools
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PurchaseOrderTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool(Name = "list_purchase_orders")]
        public async Task<IEnumerable<PurchaseOrderMcpDto>?> ListPurchaseOrders()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<PurchaseOrderMcpDto>>("api/purchase-orders");
        }

        [McpServerTool(Name = "get_purchase_order_by_id")]
        public async Task<PurchaseOrderMcpDto?> GetPurchaseOrderById(int id)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<PurchaseOrderMcpDto>($"api/purchase-orders/{id}");
        }

        [McpServerTool(Name = "search_purchase_orders")]
        public async Task<IEnumerable<PurchaseOrderMcpDto>?> SearchPurchaseOrders(string query)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<PurchaseOrderMcpDto>>(
                $"api/purchase-orders?query={Uri.EscapeDataString(query)}");
        }

        [McpServerTool(Name = "get_pending_purchase_orders")]
        public async Task<IEnumerable<PurchaseOrderMcpDto>?> GetPendingPurchaseOrders()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            var purchaseOrders = await client.GetFromJsonAsync<List<PurchaseOrderMcpDto>>("api/purchase-orders");

            return purchaseOrders?
                .Where(po =>
                    po.Status == WarehouseManagementSystem.Model.OrderStatus.Pending ||
                    po.Status == WarehouseManagementSystem.Model.OrderStatus.Approved)
                .OrderBy(po => po.ExpectedDeliveryDate)
                .ToList();
        }

        public class PurchaseOrderMcpDto
        {
            public int Id { get; set; }
            public int OrderNumber { get; set; }
            public decimal TotalAmount { get; set; }
            public WarehouseManagementSystem.Model.OrderStatus Status { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime ExpectedDeliveryDate { get; set; }

            public SupplierSummaryMcpDto? Supplier { get; set; }
            public WarehouseSummaryMcpDto? Warehouse { get; set; }
        }

        public class SupplierSummaryMcpDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class WarehouseSummaryMcpDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
        }
    }
}
