using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Model
{
    public class PurchaseOrder
    {
        public int Id { get; set; }

        public int OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime ExpectedDeliveryDate { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        public int SupplierId { get; set; }

        public Supplier Supplier { get; set; } = null!;

        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; } = null!;

        public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}
