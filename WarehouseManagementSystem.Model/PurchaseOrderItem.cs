using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WarehouseManagementSystem.Model
{
    public class PurchaseOrderItem
    {
        [Key]
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [ForeignKey(nameof(PurchaseOrder))]
        [Range(1, int.MaxValue)]
        public int PurchaseOrderId { get; set; }

        [ValidateNever]
        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        [ForeignKey(nameof(Product))]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [ValidateNever]
        public Product Product { get; set; } = null!;
    }
}
