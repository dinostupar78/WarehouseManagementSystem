using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Web.Dtos
{
    public class PurchaseOrderItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public PurchaseOrderSummaryDto? PurchaseOrder { get; set; }
        public ProductSummaryDto? Product { get; set; }
    }

    public class PurchaseOrderItemCreateDto
    {
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int PurchaseOrderId { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
    }

    public class PurchaseOrderItemUpdateDto
    {
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int PurchaseOrderId { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
    }
}
