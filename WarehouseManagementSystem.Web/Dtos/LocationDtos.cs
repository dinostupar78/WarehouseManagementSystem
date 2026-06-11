using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Web.Dtos
{
    public class LocationDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public int ShelfNumber { get; set; }
        public WarehouseSummaryDto? Warehouse { get; set; }
    }

    public class LocationCreateDto
    {
        [Required]
        [StringLength(40)]
        public string Code { get; set; } 

        [Required]
        [StringLength(40)]
        public string Zone { get; set; } 

        [Range(1, int.MaxValue)]
        public int ShelfNumber { get; set; }

        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }
    }

    public class LocationUpdateDto
    {
        [Required]
        [StringLength(40)]
        public string Code { get; set; }

        [Required]
        [StringLength(40)]
        public string Zone { get; set; }

        [Range(1, int.MaxValue)]
        public int ShelfNumber { get; set; }

        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }
    }
}
