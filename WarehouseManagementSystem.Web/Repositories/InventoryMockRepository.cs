using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Repositories
{
    public class InventoryMockRepository
    {
        private readonly List<Inventory> _inventories;

        public InventoryMockRepository(ProductMockRepository productRepository, LocationMockRepository locationRepository)
        {
            var productsById = productRepository.GetAll().ToDictionary(p => p.Id);
            var locationsById = locationRepository.GetAll().ToDictionary(l => l.Id);

            _inventories = new List<Inventory>
            {
                new Inventory
                {
                    Id = 1,
                    ProductId = 1,
                    LocationId = 1,
                    Quantity = 1,
                    LastUpdated = new DateTime(2026, 03, 30)
                },
                new Inventory
                {
                    Id = 2,
                    ProductId = 2,
                    LocationId = 2,
                    Quantity = 5,
                    LastUpdated = new DateTime(2026, 03, 27)
                },
                new Inventory
                {
                    Id = 3,
                    ProductId = 3,
                    LocationId = 3,
                    Quantity = 25,
                    LastUpdated = new DateTime(2026, 03, 23)
                },
                new Inventory
                {
                    Id = 4,
                    ProductId = 4,
                    LocationId = 4,
                    Quantity = 100,
                    LastUpdated = new DateTime(2026, 03, 25)
                },
                new Inventory
                {
                    Id = 5,
                    ProductId = 5,
                    LocationId = 3,
                    Quantity = 255,
                    LastUpdated = new DateTime(2026, 03, 22)
                },
                new Inventory
                {
                    Id = 6,
                    ProductId = 6,
                    LocationId = 3,
                    Quantity = 500,
                    LastUpdated = new DateTime(2026, 03, 26)
                }
            };

            foreach(var inventory in _inventories)
            {
                if (!productsById.TryGetValue(inventory.ProductId, out var product))
                {
                    throw new InvalidOperationException($"ProductId {inventory.ProductId} does not exist in product seed data.");
                }

                if (!locationsById.TryGetValue(inventory.LocationId, out var location))
                {
                    throw new InvalidOperationException($"LocationId {inventory.LocationId} does not exist in location seed data.");
                }

                inventory.Product = product;
                inventory.Location = location;
                product.Inventories.Add(inventory);
                location.Inventories.Add(inventory);
            }
        }

        public IReadOnlyList<Inventory> GetAll()
        {
            return _inventories;
        }

        public Inventory? GetById(int id)
        {
            return _inventories.FirstOrDefault(i => i.Id == id);
        }
    }
}