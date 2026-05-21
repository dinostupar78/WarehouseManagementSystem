using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WarehouseManagementSystem.Model
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Code { get; set; }

        [Required, MaxLength(45)]
        public string Zone { get; set; }

        [Range(1, int.MaxValue)]
        public int ShelfNumber { get; set; }

        [ForeignKey(nameof(Warehouse))]
        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }

        [ValidateNever]
        public Warehouse Warehouse { get; set; } = null!;

        [ValidateNever]
        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
