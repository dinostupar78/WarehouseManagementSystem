using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string ContactPerson { get; set; }

        [Required, EmailAddress, MaxLength(200)]
        public string ContactEmail { get; set; }

        [Required, Phone, MaxLength(100)]
        public string ContactPhone { get; set; }

        [Required, MaxLength(100)]
        public string ContactAddress { get; set; }

        public List<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}
