using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Model
{
    public class Location
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(45)]
        public string Zone { get; set; } = string.Empty;

        public int ShelfNumber { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public List<Inventory> Inventories { get; set; } = new List<Inventory>();

    }
}
