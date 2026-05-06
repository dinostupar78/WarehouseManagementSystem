using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Repositories
{
    public class PurchaseOrderMockRepository
    {
        private readonly List<PurchaseOrder> _purchaseOrders;

        public PurchaseOrderMockRepository(SupplierMockRepository supplierRepository, 
            WarehouseMockRepository warehouseRepository)
        {
            var suppliersById = supplierRepository.GetAll().ToDictionary(s => s.Id);
            var warehousesById = warehouseRepository.GetAll().ToDictionary(w => w.Id);

            _purchaseOrders = new List<PurchaseOrder>
            {
                new PurchaseOrder
                {
                    Id = 1,
                    OrderNumber = 1001,
                    OrderDate = new DateTime(2026, 03, 02),
                    ExpectedDeliveryDate = new DateTime(2026, 03, 09),
                    Status = OrderStatus.Shipped,
                    SupplierId = 1,
                    WarehouseId = 1
                },
                new PurchaseOrder
                {
                    Id = 2,
                    OrderNumber = 1002,
                    OrderDate = new DateTime(2026, 03, 08),
                    ExpectedDeliveryDate = new DateTime(2026, 03, 16),
                    Status = OrderStatus.Shipped,
                    SupplierId = 2,
                    WarehouseId = 2
                },
                new PurchaseOrder
                {
                    Id = 3,
                    OrderNumber = 1003,
                    OrderDate = new DateTime(2026, 03, 12),
                    ExpectedDeliveryDate = new DateTime(2026, 03, 21),
                    Status = OrderStatus.Delivered,
                    SupplierId = 3,
                    WarehouseId = 3
                }
            };

            foreach(var purchaseOrder in _purchaseOrders)
            {
                if (!suppliersById.TryGetValue(purchaseOrder.SupplierId, out var supplier))
                {
                    throw new InvalidOperationException($"SupplierId {purchaseOrder.SupplierId} does not exist in supplier seed data.");
                }

                if (!warehousesById.TryGetValue(purchaseOrder.WarehouseId, out var warehouse))
                {
                    throw new InvalidOperationException($"WarehouseId {purchaseOrder.WarehouseId} does not exist in warehouse seed data.");
                }

                purchaseOrder.Supplier = supplier;
                purchaseOrder.Warehouse = warehouse;
                supplier.PurchaseOrders.Add(purchaseOrder);
                warehouse.PurchaseOrders.Add(purchaseOrder);
            }
        }

        public IReadOnlyList<PurchaseOrder> GetAll()
        {
            return _purchaseOrders;
        }

        public PurchaseOrder? GetById(int id)
        {
            return _purchaseOrders.FirstOrDefault(po => po.Id == id);
        }
    }
}   