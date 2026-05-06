using WarehouseManagementSystem.Model;
namespace WarehouseManagementSystem.Repositories
{
    public class LocationMockRepository
    {
        private readonly List<Location> _locations;

        public LocationMockRepository(WarehouseMockRepository warehouseRepository)
        {
            var warehousesById = warehouseRepository.GetAll().ToDictionary(w => w.Id);

            _locations = new List<Location>
            {
               new Location
                {
                    Id = 1,
                    Code = "MDC-A-01",
                    Zone = "A",
                    ShelfNumber = 1,
                    WarehouseId = 1
                },
                new Location
                {
                    Id = 2,
                    Code = "MDC-B-03",
                    Zone = "B",
                    ShelfNumber = 3,
                    WarehouseId = 1
                },
                new Location
                {
                    Id = 3,
                    Code = "EFH-A-01",
                    Zone = "A",
                    ShelfNumber = 1,
                    WarehouseId = 2
                },
                new Location
                {
                    Id = 4,
                    Code = "EFH-C-06",  
                    Zone = "C",
                    ShelfNumber = 6,
                    WarehouseId = 2
                },
            };

            foreach (var location in _locations)
            {
                if (!warehousesById.TryGetValue(location.WarehouseId, out var warehouse))
                {
                    throw new InvalidOperationException($"WarehouseId {location.WarehouseId} does not exist in warehouse seed data.");
                }

                location.Warehouse = warehouse;
                warehouse.Locations.Add(location);
            }
        }
        public IReadOnlyList<Location> GetAll()
        {
            return _locations;
        }

        public Location? GetById(int id)
        {
            return _locations.FirstOrDefault(l => l.Id == id);
        }
    }
}
