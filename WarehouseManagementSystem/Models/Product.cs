using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(255)]
        public string Description { get; set; }

        public decimal Price { get; set; }

        public decimal Weight { get; set; }

        public DateTime ProductReceivedAt { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public List<Inventory> Inventories { get; set; } = new List<Inventory>();
        public List<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();


    }
}
