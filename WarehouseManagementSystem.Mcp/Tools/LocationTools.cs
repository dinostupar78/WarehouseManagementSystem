using ModelContextProtocol.Server;
using System.Net.Http.Json;
using static WarehouseManagementSystem.Mcp.Tools.PurchaseOrderTools;

namespace WarehouseManagementSystem.Mcp.Tools
{
    [McpServerToolType]
    public class LocationTools
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LocationTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool(Name = "list_locations")]
        public async Task<IEnumerable<LocationMcpDto>?> ListLocations()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<LocationMcpDto>>("api/locations");
        }

        [McpServerTool(Name = "get_location_by_id")]
        public async Task<LocationMcpDto?> GetLocationById(int id)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<LocationMcpDto>($"api/locations/{id}");
        }

        [McpServerTool(Name = "search_locations")]
        public async Task<IEnumerable<LocationMcpDto>?> SearchLocations(string query)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<LocationMcpDto>>(
                $"api/locations?query={Uri.EscapeDataString(query)}");
        }

        [McpServerTool(Name = "get_locations_by_zone")]
        public async Task<IEnumerable<LocationMcpDto>?> GetLocationsByZone(string zone)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");
            var locations = await client.GetFromJsonAsync<List<LocationMcpDto>>("api/locations");

            return locations?
                .Where(l => l.Zone.Contains(zone, StringComparison.OrdinalIgnoreCase))
                .OrderBy(l => l.Warehouse?.Name)
                .ThenBy(l => l.ShelfNumber)
                .ToList();
        }

        public class LocationMcpDto
        {
            public int Id { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Zone { get; set; } = string.Empty;
            public int ShelfNumber { get; set; }
            public WarehouseSummaryMcpDto? Warehouse { get; set; }
        }
    }
}
