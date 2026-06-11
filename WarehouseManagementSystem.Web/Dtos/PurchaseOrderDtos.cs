using System.ComponentModel.DataAnnotations;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Dtos
{
    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public SupplierSummaryDto? Supplier { get; set; }
        public WarehouseSummaryDto? Warehouse { get; set; }
    }

    public class PurchaseOrderCreateDto
    {
        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public DateTime ExpectedDeliveryDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        [Range(1, int.MaxValue)]
        public int SupplierId { get; set; }

        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }
    }

    public class PurchaseOrderUpdateDto
    {
        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public DateTime ExpectedDeliveryDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        [Range(1, int.MaxValue)]
        public int SupplierId { get; set; }

        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }
    }
}
