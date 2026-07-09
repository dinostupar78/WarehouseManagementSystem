using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using static WarehouseManagementSystem.Web.Models.AI.EntityAiSuggestionsModel;

namespace WarehouseManagementSystem.Web.Services.AI
{
    public class AiReferenceMatcher
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;

        public AiReferenceMatcher(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task MatchProductAsync(ProductAiSuggestion suggestion)
        {
            if (string.IsNullOrWhiteSpace(suggestion.CategoryName))
            {
                return;
            }

            var categoryName = suggestion.CategoryName.Trim().ToLower();

            var category = await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName);

            if (category == null)
            {
                suggestion.Message = $"Category '{suggestion.CategoryName}' was not found. Please select it manually.";
                return;
            }

            suggestion.CategoryId = category.Id;
        }

        public async Task MatchLocationAsync(LocationAiSuggestion suggestion)
        {
            if (string.IsNullOrWhiteSpace(suggestion.WarehouseName))
            {
                return;
            }

            var warehouseName = suggestion.WarehouseName.Trim().ToLower();

            var warehouse = await _dbContext.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Name.ToLower() == warehouseName);

            if (warehouse == null)
            {
                suggestion.Message = $"Warehouse '{suggestion.WarehouseName}' was not found. Please select it manually.";
                return;
            }

            suggestion.WarehouseId = warehouse.Id;
        }

        public async Task MatchInventoryAsync(InventoryAiSuggestion suggestion)
        {
            if (!string.IsNullOrWhiteSpace(suggestion.ProductName))
            {
                var productName = suggestion.ProductName.Trim().ToLower();

                var product = await _dbContext.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == productName);

                if (product != null)
                {
                    suggestion.ProductId = product.Id;
                }
                else
                {
                    suggestion.Message = $"Product '{suggestion.ProductName}' was not found. Please select it manually.";
                }
            }

            if (!string.IsNullOrWhiteSpace(suggestion.LocationCode))
            {
                var locationCode = suggestion.LocationCode.Trim().ToLower();

                var location = await _dbContext.Locations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Code.ToLower() == locationCode);

                if (location != null)
                {
                    suggestion.LocationId = location.Id;
                }
                else
                {
                    suggestion.Message = $"Location '{suggestion.LocationCode}' was not found. Please select it manually.";
                }
            }
        }

        public async Task MatchPurchaseOrderAsync(PurchaseOrderAiSuggestion suggestion)
        {
            if (!string.IsNullOrWhiteSpace(suggestion.SupplierName))
            {
                var supplierName = suggestion.SupplierName.Trim().ToLower();

                var supplier = await _dbContext.Suppliers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == supplierName);

                if (supplier != null)
                {
                    suggestion.SupplierId = supplier.Id;
                }
                else
                {
                    suggestion.Message = $"Supplier '{suggestion.SupplierName}' was not found. Please select it manually.";
                }
            }

            if (!string.IsNullOrWhiteSpace(suggestion.WarehouseName))
            {
                var warehouseName = suggestion.WarehouseName.Trim().ToLower();

                var warehouse = await _dbContext.Warehouses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Name.ToLower() == warehouseName);

                if (warehouse != null)
                {
                    suggestion.WarehouseId = warehouse.Id;
                }
                else
                {
                    suggestion.Message = $"Warehouse '{suggestion.WarehouseName}' was not found. Please select it manually.";
                }
            }
        }

        public async Task MatchPurchaseOrderItemAsync(PurchaseOrderItemAiSuggestion suggestion)
        {
            var orderNumber = ExtractOrderNumber(suggestion.PurchaseOrderNumber);

            if (orderNumber.HasValue)
            {
                var purchaseOrder = await _dbContext.PurchaseOrders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(po => po.OrderNumber == orderNumber.Value);

                if (purchaseOrder != null)
                {
                    suggestion.PurchaseOrderId = purchaseOrder.Id;
                }
                else
                {
                    suggestion.Message = $"Purchase order PO-{orderNumber.Value} was not found. Please select it manually.";
                }
            }

            if (!string.IsNullOrWhiteSpace(suggestion.ProductName))
            {
                var productName = suggestion.ProductName.Trim().ToLower();

                var product = await _dbContext.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == productName);

                if (product != null)
                {
                    suggestion.ProductId = product.Id;
                }
                else
                {
                    suggestion.Message = $"Product '{suggestion.ProductName}' was not found. Please select it manually.";
                }
            }
        }

        private static int? ExtractOrderNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var digits = new string(value.Where(char.IsDigit).ToArray());

            return int.TryParse(digits, out var orderNumber) ? orderNumber : null;
        }
    }
}
