using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Web.Dtos
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int Capacity { get; set; }
        public List<LocationSummaryDto> Locations { get; set; } = new List<LocationSummaryDto>();
    }

    public class WarehouseCreateDto
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
    }

    public class WarehouseUpdateDto
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
    }
}
