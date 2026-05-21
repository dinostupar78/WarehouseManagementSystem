using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WarehouseManagementSystem.Model
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(200)]
        public string Address { get; set; }

        [Required, MaxLength(200)]
        public string City { get; set; }

        [Required, MaxLength(100)]
        public string Country { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        [ValidateNever]
        public virtual ICollection<Location> Locations { get; set; } = new List<Location>();

        [ValidateNever]
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}
