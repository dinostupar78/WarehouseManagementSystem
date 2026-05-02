using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Models
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(200)]
        public string Address { get; set; }

        [Required, MaxLength(200)]
        public string City { get; set; }

        [Required, MaxLength(100)]
        public string Country { get; set; }

        public int Capacity { get; set; }

        public List<Location> Locations { get; set; } = new List<Location>();

        // Opcionalno za sad, ali može biti korisno za praćenje zaliha i narudžbi
        public List<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    }
}
