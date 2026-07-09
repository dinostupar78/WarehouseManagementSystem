using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace WarehouseManagementSystem.Mcp.Tools
{
    [McpServerToolType]
    public class SupplierTools
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SupplierTools(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool(Name = "list_suppliers")]
        public async Task<IEnumerable<SupplierMcpDto>?> ListSuppliers()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<SupplierMcpDto>>("api/suppliers");
        }

        [McpServerTool(Name = "get_supplier_by_id")]
        public async Task<SupplierMcpDto?> GetSupplierById(int id)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<SupplierMcpDto>($"api/suppliers/{id}");
        }

        [McpServerTool(Name = "search_suppliers")]
        public async Task<IEnumerable<SupplierMcpDto>?> SearchSuppliers(string query)
        {
            var client = _httpClientFactory.CreateClient("WmsApi");

            return await client.GetFromJsonAsync<IEnumerable<SupplierMcpDto>>(
                $"api/suppliers?query={Uri.EscapeDataString(query)}");
        }

        [McpServerTool(Name = "get_supplier_contact_overview")]
        public async Task<SupplierContactOverviewDto?> GetSupplierContactOverview()
        {
            var client = _httpClientFactory.CreateClient("WmsApi");
            var suppliers = await client.GetFromJsonAsync<List<SupplierMcpDto>>("api/suppliers");

            if (suppliers == null)
            {
                return null;
            }

            return new SupplierContactOverviewDto
            {
                TotalSuppliers = suppliers.Count,
                SuppliersWithEmail = suppliers.Count(s => !string.IsNullOrWhiteSpace(s.Email)),
                SuppliersWithoutEmail = suppliers
                    .Where(s => string.IsNullOrWhiteSpace(s.Email))
                    .OrderBy(s => s.Name)
                    .ToList()
            };
        }

        public class SupplierMcpDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string ContactPerson { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }

        public class SupplierContactOverviewDto
        {
            public int TotalSuppliers { get; set; }
            public int SuppliersWithEmail { get; set; }
            public List<SupplierMcpDto> SuppliersWithoutEmail { get; set; } = new();
        }
    }
}
