using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int ShelfNumber { get; set; }

        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; } = null!;

        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
