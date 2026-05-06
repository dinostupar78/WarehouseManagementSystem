using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseManagementSystem.Model
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        public int Quantity { get; set; }

        public DateTime LastUpdated { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        [ForeignKey(nameof(Location))]
        public int LocationId { get; set; }

        public Location Location { get; set; } = null!;
    }
}
