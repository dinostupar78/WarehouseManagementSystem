using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Models;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class DashboardRepository
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;

        public DashboardRepository(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var model = new DashboardViewModel
            {
                TotalProducts = await _dbContext.Products.CountAsync(),
                ActiveSuppliers = await _dbContext.Suppliers.CountAsync(),
                PendingOrders = await _dbContext.PurchaseOrders
                    .CountAsync(po => po.Status == OrderStatus.Pending)
            };

            model.RecentInventory = await GetRecentInventoryAsync();
            model.LowStockProducts = await GetLowStockProductsAsync();
            model.PendingPurchaseOrders = await GetPendingOrdersAsync();
            model.WarehouseCapacities = await GetWarehouseCapacitiesAsync();

            model.LowStockItems = model.LowStockProducts.Count;

            if (model.WarehouseCapacities.Any())
            {
                var totalCapacity = model.WarehouseCapacities.Sum(w => w.Capacity);
                var totalUsed = model.WarehouseCapacities.Sum(w => w.UsedCapacity);

                model.TotalCapacityUsedPercent = totalCapacity == 0
                    ? 0
                    : Math.Round((decimal)totalUsed / totalCapacity * 100, 1);

                model.TotalCapacityFree = Math.Max(totalCapacity - totalUsed, 0);
            }

            return model;
        }

        private async Task<List<RecentInventoryViewModel>> GetRecentInventoryAsync()
        {
            return await _dbContext.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Location)
                    .ThenInclude(l => l.Warehouse)
                .OrderByDescending(i => i.LastUpdated)
                .Take(5)
                .Select(i => new RecentInventoryViewModel
                {
                    InventoryId = i.Id,
                    ProductName = i.Product.Name,
                    LocationCode = i.Location.Code,
                    WarehouseName = i.Location.Warehouse.Name,
                    Quantity = i.Quantity,
                    LastUpdated = i.LastUpdated,
                    StockStatus = i.Quantity <= 0
                        ? "OUT OF STOCK"
                        : i.Quantity <= 10
                            ? "LOW STOCK"
                            : "IN STOCK"
                })
                .ToListAsync();
        }

        private async Task<List<LowStockViewModel>> GetLowStockProductsAsync()
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Select(p => new LowStockViewModel
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    TotalQuantity = p.Inventories.Sum(i => i.Quantity)
                })
                .Where(p => p.TotalQuantity <= 10)
                .OrderBy(p => p.TotalQuantity)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<PendingOrderViewModel>> GetPendingOrdersAsync()
        {
            var orders = await _dbContext.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Where(po => po.Status == OrderStatus.Pending || po.Status == OrderStatus.Shipped)
                .OrderBy(po => po.ExpectedDeliveryDate)
                .Take(5)
                .Select(po => new PendingOrderViewModel
                {
                    PurchaseOrderId = po.Id,
                    OrderNumber = po.OrderNumber,
                    SupplierName = po.Supplier.Name,
                    WarehouseName = po.Warehouse.Name,
                    ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                    Status = po.Status.ToString(),
                    IsDelayed = false
                })
                .ToListAsync();

            foreach (var order in orders)
            {
                order.IsDelayed = order.ExpectedDeliveryDate.Date < DateTime.Today;
            }

            return orders;
        }

        private async Task<List<WarehouseCapacityViewModel>> GetWarehouseCapacitiesAsync()
        {
            var warehouses = await _dbContext.Warehouses
                .AsNoTracking()
                .Select(w => new WarehouseCapacityViewModel
                {
                    WarehouseId = w.Id,
                    WarehouseName = w.Name,
                    Capacity = w.Capacity,
                    UsedCapacity = w.Locations
                        .SelectMany(l => l.Inventories)
                        .Sum(i => i.Quantity)
                })
                .OrderByDescending(w => w.UsedCapacity)
                .Take(5)
                .ToListAsync();

            foreach (var warehouse in warehouses)
            {
                warehouse.UsedPercent = warehouse.Capacity == 0
                    ? 0
                    : Math.Round((decimal)warehouse.UsedCapacity / warehouse.Capacity * 100, 1);
            }

            return warehouses;
        }

    }
}
