using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WarehouseManagementSystem.Model
{
    public class PurchaseOrder : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int OrderNumber { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ExpectedDeliveryDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        [ForeignKey(nameof(Supplier))]
        [Range(1, int.MaxValue)]
        public int SupplierId { get; set; }

        [ValidateNever]
        public Supplier Supplier { get; set; } = null!;

        [ForeignKey(nameof(Warehouse))]
        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }

        [ValidateNever]
        public Warehouse Warehouse { get; set; } = null!;

        [ValidateNever]
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ExpectedDeliveryDate < OrderDate)
            {
                yield return new ValidationResult(
                    "Expected delivery date cannot be before the order date.",
                    new[] { nameof(ExpectedDeliveryDate) });
            }
        }
    }
}
