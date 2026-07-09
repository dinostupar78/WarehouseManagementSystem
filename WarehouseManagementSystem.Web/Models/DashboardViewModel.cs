namespace WarehouseManagementSystem.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int ActiveSuppliers { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockItems { get; set; }

        public decimal TotalCapacityUsedPercent { get; set; }
        public int TotalCapacityFree { get; set; }

        public List<RecentInventoryViewModel> RecentInventory { get; set; } = new();
        public List<LowStockViewModel> LowStockProducts { get; set; } = new();
        public List<PendingOrderViewModel> PendingPurchaseOrders { get; set; } = new();
        public List<WarehouseCapacityViewModel> WarehouseCapacities { get; set; } = new();
    }

    public class RecentInventoryViewModel
    {
        public int InventoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
        public string StockStatus { get; set; } = string.Empty;

        public string StockBadgeClass => StockStatus switch
        {
            "IN STOCK" => "wms-badge-success",
            "LOW STOCK" => "wms-badge-warning",
            "OUT OF STOCK" => "wms-badge-danger",
            _ => "wms-badge-primary"
        };
    }

    public class LowStockViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public string StockText => TotalQuantity <= 0 ? "OUT OF STOCK" : "LOW STOCK";
        public string StockBadgeClass => TotalQuantity <= 0 ? "wms-badge-danger" : "wms-badge-warning";
    }

    public class PendingOrderViewModel
    {
        public int PurchaseOrderId { get; set; }
        public int OrderNumber { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime ExpectedDeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsDelayed { get; set; }

        public string StatusText => IsDelayed ? "DELAYED" : Status.ToUpper();

        public string StatusBadgeClass
        {
            get
            {
                if (IsDelayed)
                {
                    return "wms-badge-danger";
                }

                return Status switch
                {
                    "Approved" => "wms-badge-primary",
                    "Shipped" => "wms-badge-info",
                    "Delivered" => "wms-badge-success",
                    "Cancelled" => "wms-badge-danger",
                    _ => "wms-badge-warning"
                };
            }
        }
    }

    public class WarehouseCapacityViewModel
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int UsedCapacity { get; set; }
        public decimal UsedPercent { get; set; }

        public string CapacityBarClass
        {
            get
            {
                if (UsedPercent >= 85)
                {
                    return "bg-danger";
                }

                if (UsedPercent >= 65)
                {
                    return "bg-warning";
                }

                return "bg-success";
            }
        }
    }
}
