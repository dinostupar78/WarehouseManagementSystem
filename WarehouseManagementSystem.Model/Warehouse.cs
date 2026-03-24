using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Model
{
    public class Warehouse
    {
       
        public int Id { get; set; }
        
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required, MaxLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [Required, MaxLength(200)]
        public string City { get; set; } = string.Empty;
        
        [Required, MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public List<Location> Locations { get; set; } = new List<Location>();

        // Opcionalno za sad, ali može biti korisno za praćenje zaliha i narudžbi
        public List<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();


    }
}
