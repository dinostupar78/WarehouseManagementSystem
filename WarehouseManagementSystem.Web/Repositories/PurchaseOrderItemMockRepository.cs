using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public class PurchaseOrderItemMockRepository
    {
        private readonly List<PurchaseOrderItem> _purchaseOrderItems;

        public PurchaseOrderItemMockRepository(ProductMockRepository productRepository, PurchaseOrderMockRepository purchaseOrderRepository)
        {
            var productsById = productRepository.GetAll().ToDictionary(p => p.Id);
            var purchaseOrdersById = purchaseOrderRepository.GetAll().ToDictionary(po => po.Id);
            _purchaseOrderItems = new List<PurchaseOrderItem>
            {
                new PurchaseOrderItem
                {
                    Id = 1,
                    PurchaseOrderId = 1,
                    ProductId = 1,
                    Quantity = 50,
                    UnitPrice = 120m
                },
                new PurchaseOrderItem
                {
                    Id = 2,
                    PurchaseOrderId = 1,
                    ProductId = 2,
                    Quantity = 100,
                    UnitPrice = 230m
                },
                new PurchaseOrderItem
                {
                    Id = 3,
                    PurchaseOrderId = 2,
                    ProductId = 3,
                    Quantity = 30,
                    UnitPrice = 1500m
                },
                new PurchaseOrderItem
                {
                    Id = 4,
                    PurchaseOrderId = 2,
                    ProductId = 4,
                    Quantity = 20,
                    UnitPrice = 300m
                },
                new PurchaseOrderItem
                {
                    Id = 5,
                    PurchaseOrderId = 3,
                    ProductId = 5,
                    Quantity = 40,
                    UnitPrice = 250m
                },
                new PurchaseOrderItem
                {
                    Id = 6,
                    PurchaseOrderId = 3,
                    ProductId = 6,
                    Quantity = 25,
                    UnitPrice = 400m
                }
            };

            foreach (var purchaseOrderItem in _purchaseOrderItems)
            {
                if (!productsById.TryGetValue(purchaseOrderItem.ProductId, out var product))
                {
                    throw new InvalidOperationException($"ProductId {purchaseOrderItem.ProductId} does not exist in product seed data.");
                }
                if(!purchaseOrdersById.TryGetValue(purchaseOrderItem.PurchaseOrderId, out var purchaseOrder))
                {
                    throw new InvalidOperationException($"PurchaseOrderId {purchaseOrderItem.PurchaseOrderId} does not exist in purchase order seed data.");
                }
                purchaseOrderItem.Product = product;
                purchaseOrderItem.PurchaseOrder = purchaseOrder;
                product.PurchaseOrderItems.Add(purchaseOrderItem);
                purchaseOrder.Items.Add(purchaseOrderItem);

            }

            foreach (var purchaseOrder in purchaseOrdersById.Values)
            {
                purchaseOrder.TotalAmount = purchaseOrder.Items.Sum(i => i.Quantity * i.UnitPrice);
            }
        }

        public IReadOnlyList<PurchaseOrderItem> GetAll()
        {
            return _purchaseOrderItems;
        }

        public PurchaseOrderItem GetById(int id)
        {
            return _purchaseOrderItems.FirstOrDefault(i => i.Id == id);
        }
    }
}