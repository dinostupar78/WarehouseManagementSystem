using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Models;

namespace WarehouseManagementSystem.Web.Controllers
{
    [Route("global-search")]
    public class GlobalSearchController : Controller
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;

        public GlobalSearchController(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? term)
        {
            var results = new List<GlobalSearchResultModel>();

            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(results);
            }

            term = term.Trim().ToLower();
            var orderTerm = term.Replace("po-", "").Replace("po", "");

            AddStaticResults(results, term);

            var products = await _dbContext.Products
                .AsNoTracking()
                .Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term))
                .Take(5)
                .Select(p => new GlobalSearchResultModel
                {
                    Title = p.Name,
                    Subtitle = "Product",
                    Type = "Product",
                    Url = "/products/" + p.Id
                })
                .ToListAsync();

            results.AddRange(products);

            var warehouses = await _dbContext.Warehouses
                .AsNoTracking()
                .Where(w =>
                    w.Name.ToLower().Contains(term) ||
                    w.City.ToLower().Contains(term) ||
                    w.Country.ToLower().Contains(term))
                .Take(5)
                .Select(w => new GlobalSearchResultModel
                {
                    Title = w.Name,
                    Subtitle = w.City + ", " + w.Country,
                    Type = "Warehouse",
                    Url = "/warehouses/" + w.Id
                })
                .ToListAsync();

            results.AddRange(warehouses);

            var suppliers = await _dbContext.Suppliers
                .AsNoTracking()
                .Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    s.ContactEmail.ToLower().Contains(term) ||
                    s.ContactPhone.ToLower().Contains(term))
                .Take(5)
                .Select(s => new GlobalSearchResultModel
                {
                    Title = s.Name,
                    Subtitle = s.ContactEmail,
                    Type = "Supplier",
                    Url = "/suppliers/" + s.Id
                })
                .ToListAsync();

            results.AddRange(suppliers);

            var categories = await _dbContext.Categories
                .AsNoTracking()
                .Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    c.Description.ToLower().Contains(term))
                .Take(5)
                .Select(c => new GlobalSearchResultModel
                {
                    Title = c.Name,
                    Subtitle = "Category",
                    Type = "Category",
                    Url = "/categories/" + c.Id
                })
                .ToListAsync();

            results.AddRange(categories);

            var locations = await _dbContext.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .Where(l =>
                    l.Code.ToLower().Contains(term) ||
                    l.Zone.ToLower().Contains(term) ||
                    l.Warehouse.Name.ToLower().Contains(term))
                .Take(5)
                .Select(l => new GlobalSearchResultModel
                {
                    Title = l.Code,
                    Subtitle = l.Zone + " · " + l.Warehouse.Name,
                    Type = "Location",
                    Url = "/locations/" + l.Id
                })
                .ToListAsync();

            results.AddRange(locations);

            var inventories = await _dbContext.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Location)
                .ThenInclude(l => l.Warehouse)
                .Where(i =>
                    i.Id.ToString().Contains(term) ||
                    i.Product.Name.ToLower().Contains(term) ||
                    i.Location.Code.ToLower().Contains(term) ||
                    i.Location.Warehouse.Name.ToLower().Contains(term))
                .Take(5)
                .Select(i => new GlobalSearchResultModel
                {
                    Title = "INV-" + i.Id.ToString("0000"),
                    Subtitle = i.Product.Name + " · " + i.Quantity + " units",
                    Type = "Inventory",
                    Url = "/inventories/" + i.Id
                })
                .ToListAsync();

            results.AddRange(inventories);

            var purchaseOrders = await _dbContext.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Where(po =>
                    po.OrderNumber.ToString().Contains(orderTerm) ||
                    po.Supplier.Name.ToLower().Contains(term) ||
                    po.Warehouse.Name.ToLower().Contains(term))
                .Take(5)
                .Select(po => new GlobalSearchResultModel
                {
                    Title = "PO-" + po.OrderNumber.ToString("0000"),
                    Subtitle = po.Supplier.Name + " · " + po.Status,
                    Type = "Purchase Order",
                    Url = "/purchase-orders/" + po.Id
                })
                .ToListAsync();

            results.AddRange(purchaseOrders);

            var purchaseOrderItems = await _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Include(item => item.Product)
                .Include(item => item.PurchaseOrder)
                .Where(item =>
                    item.Id.ToString().Contains(term) ||
                    item.Product.Name.ToLower().Contains(term) ||
                    item.PurchaseOrder.OrderNumber.ToString().Contains(orderTerm))
                .Take(5)
                .Select(item => new GlobalSearchResultModel
                {
                    Title = "POI-" + item.Id.ToString("0000"),
                    Subtitle = item.Product.Name + " · PO-" + item.PurchaseOrder.OrderNumber.ToString("0000"),
                    Type = "Order Item",
                    Url = "/purchase-order-items/" + item.Id
                })
                .ToListAsync();

            results.AddRange(purchaseOrderItems);

            return Json(results.Take(20));
        }

        private static void AddStaticResults(List<GlobalSearchResultModel> results, string term)
        {
            var staticResults = new List<GlobalSearchResultModel>
            {
                new() { Title = "Dashboard", Subtitle = "Page", Type = "Page", Url = "/" },
                new() { Title = "Warehouses", Subtitle = "Page", Type = "Page", Url = "/warehouses" },
                new() { Title = "Products", Subtitle = "Page", Type = "Page", Url = "/products" },
                new() { Title = "Inventory", Subtitle = "Page", Type = "Page", Url = "/inventories" },
                new() { Title = "Suppliers", Subtitle = "Page", Type = "Page", Url = "/suppliers" },
                new() { Title = "Purchase Orders", Subtitle = "Page", Type = "Page", Url = "/purchase-orders" },
                new() { Title = "Purchase Order Items", Subtitle = "Page", Type = "Page", Url = "/purchase-order-items" },
                new() { Title = "Locations", Subtitle = "Page", Type = "Page", Url = "/locations" },
                new() { Title = "Categories", Subtitle = "Page", Type = "Page", Url = "/categories" },

                new() { Title = "Create Product", Subtitle = "Action", Type = "Action", Url = "/products/create" },
                new() { Title = "Create Warehouse", Subtitle = "Action", Type = "Action", Url = "/warehouses/create" },
                new() { Title = "Create Supplier", Subtitle = "Action", Type = "Action", Url = "/suppliers/create" },
                new() { Title = "Create Purchase Order", Subtitle = "Action", Type = "Action", Url = "/purchase-orders/create" }
            };

            results.AddRange(staticResults.Where(r =>
                r.Title.ToLower().Contains(term) ||
                r.Subtitle.ToLower().Contains(term) ||
                r.Type.ToLower().Contains(term)));
        }
    }
}
