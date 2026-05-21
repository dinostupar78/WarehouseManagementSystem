using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WarehouseManagementSystem.Model
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime LastUpdated { get; set; }

        [ForeignKey(nameof(Product))]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [ValidateNever]
        public Product Product { get; set; } = null!;

        [ForeignKey(nameof(Location))]
        [Range(1, int.MaxValue)]
        public int LocationId { get; set; }

        [ValidateNever]
        public Location Location { get; set; } = null!;
    }
}
