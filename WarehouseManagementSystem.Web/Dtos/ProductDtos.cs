using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Web.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public DateTime ProductReceivedAt { get; set; }
        public CategoryDto Category { get; set; }
    }

    public class ProductCreateDto
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; } 

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Weight { get; set; }

        [Required]
        public DateTime ProductReceivedAt { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }

    public class ProductUpdateDto
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; } 

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Weight { get; set; }

        [Required]
        public DateTime ProductReceivedAt { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }

}
