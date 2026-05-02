using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Code { get; set; }

        [Required, MaxLength(45)]
        public string Zone { get; set; }

        public int ShelfNumber { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public List<Inventory> Inventories { get; set; } = new List<Inventory>();

    }
}
