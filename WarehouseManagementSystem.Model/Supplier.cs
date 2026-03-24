using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Model
{
    public class Supplier
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required, MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string ContactEmail { get; set; } = string.Empty;
        
        [Required, Phone, MaxLength(100)]
        public string ContactPhone { get; set; } = string.Empty;
        
        [Required, MaxLength(100)]
        public string ContactAddress { get; set; } = string.Empty;

        public List<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}
