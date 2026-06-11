using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Web.Dtos
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
        public ProductSummaryDto? Product { get; set; }
        public LocationSummaryDto? Location { get; set; }
    }

    public class InventoryCreateDto
    {
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int LocationId { get; set; }
    }

    public class InventoryUpdateDto
    {
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int LocationId { get; set; }
    }
}
