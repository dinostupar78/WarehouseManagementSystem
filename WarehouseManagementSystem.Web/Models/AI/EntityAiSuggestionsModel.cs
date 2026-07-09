namespace WarehouseManagementSystem.Web.Models.AI
{
    public class EntityAiSuggestionsModel
    {
        public class CategoryAiSuggestion
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
        }
        public class WarehouseAiSuggestion
        {
            public string? Name { get; set; }
            public string? Address { get; set; }
            public string? City { get; set; }
            public string? Country { get; set; }
            public int? Capacity { get; set; }
        }
        public class SupplierAiSuggestion
        {
            public string? Name { get; set; }
            public string? ContactPerson { get; set; }
            public string? ContactName { get; set; }
            public string? ContactEmail { get; set; }
            public string? ContactPhone { get; set; }
            public string? ContactAddress { get; set; }
        }
        public class ProductAiSuggestion
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? CategoryName { get; set; }
            public int? CategoryId { get; set; }
            public decimal? Price { get; set; }
            public decimal? Weight { get; set; }
            public DateTime? ProductReceivedAt { get; set; }
            public string? Message { get; set; }
        }
        public class LocationAiSuggestion
        {
            public string? Code { get; set; }
            public string? Zone { get; set; }
            public int? ShelfNumber { get; set; }
            public string? WarehouseName { get; set; }
            public int? WarehouseId { get; set; }
            public string? Message { get; set; }
        }
        public class InventoryAiSuggestion
        {
            public string? ProductName { get; set; }
            public int? ProductId { get; set; }
            public string? LocationCode { get; set; }
            public int? LocationId { get; set; }
            public int? Quantity { get; set; }
            public DateTime? LastUpdated { get; set; }
            public string? Message { get; set; }
        }
        public class PurchaseOrderAiSuggestion
        {
            public string? SupplierName { get; set; }
            public int? SupplierId { get; set; }
            public string? WarehouseName { get; set; }
            public int? WarehouseId { get; set; }
            public decimal? TotalAmount { get; set; }
            public string? Status { get; set; }
            public DateTime? OrderDate { get; set; }
            public DateTime? ExpectedDeliveryDate { get; set; }
            public string? Message { get; set; }
        }
        public class PurchaseOrderItemAiSuggestion
        {
            public string? PurchaseOrderNumber { get; set; }
            public int? PurchaseOrderId { get; set; }
            public string? ProductName { get; set; }
            public int? ProductId { get; set; }
            public int? Quantity { get; set; }
            public decimal? UnitPrice { get; set; }
            public string? Message { get; set; }
        }
    }
}
