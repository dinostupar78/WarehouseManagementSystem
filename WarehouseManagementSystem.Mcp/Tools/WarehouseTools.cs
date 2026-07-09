using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace WarehouseManagementSystem.Mcp.Tools
{
    [McpServerToolType]
    public class WarehouseTools
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WarehouseTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool(Name = "list_warehouses")]
        public async Task<IEnumerable<WarehouseMcpDto>?> ListWarehouses()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<WarehouseMcpDto>>("api/warehouses");
        }

        [McpServerTool(Name = "get_warehouse_by_id")]
        public async Task<WarehouseMcpDto?> GetWarehouseById(int id)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<WarehouseMcpDto>($"api/warehouses/{id}");
        }

        [McpServerTool(Name = "search_warehouses")]
        public async Task<IEnumerable<WarehouseMcpDto>?> SearchWarehouses(string query)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<WarehouseMcpDto>>(
                $"api/warehouses?query={Uri.EscapeDataString(query)}");
        }

        [McpServerTool(Name = "get_warehouse_capacity_overview")]
        public async Task<WarehouseCapacityOverviewDto?> GetWarehouseCapacityOverview()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            var warehouses = await client.GetFromJsonAsync<List<WarehouseMcpDto>>("api/warehouses");

            if (warehouses == null || warehouses.Count == 0)
            {
                return new WarehouseCapacityOverviewDto
                {
                    TotalWarehouses = 0,
                    TotalCapacity = 0,
                    LargestWarehouse = null
                };
            }

            var largestWarehouse = warehouses
                .OrderByDescending(w => w.Capacity)
                .First();

            return new WarehouseCapacityOverviewDto
            {
                TotalWarehouses = warehouses.Count,
                TotalCapacity = warehouses.Sum(w => w.Capacity),
                LargestWarehouse = largestWarehouse
            };
        }

        public class WarehouseMcpDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;
            public int Capacity { get; set; }
        }

        public class WarehouseCapacityOverviewDto
        {
            public int TotalWarehouses { get; set; }
            public int TotalCapacity { get; set; }
            public WarehouseMcpDto? LargestWarehouse { get; set; }
        }
    }
}
