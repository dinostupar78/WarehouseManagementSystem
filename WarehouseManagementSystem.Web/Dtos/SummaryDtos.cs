namespace WarehouseManagementSystem.Web.Dtos
{
    public class WarehouseSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string City { get; set; } 
    }

    public class LocationSummaryDto
    {
        public int Id { get; set; }
        public string Code { get; set; } 
        public string Zone { get; set; } 
        public int ShelfNumber { get; set; }
    }

    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
    }

    public class SupplierSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string ContactEmail { get; set; } 
    }

    public class PurchaseOrderSummaryDto
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
