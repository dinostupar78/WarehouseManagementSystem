using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WarehouseManagementSystem.Model
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(255)]
        public string Description { get; set; }

        [ValidateNever]
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
