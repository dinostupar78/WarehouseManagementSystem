using System.ComponentModel.DataAnnotations;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(255)]
        public string Description { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();

    }
}
