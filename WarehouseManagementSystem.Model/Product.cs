using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Model
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal Weight { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public List<Inventory> Inventories { get; set; } = new List<Inventory>();
        public List<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();


    }
}
