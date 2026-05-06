using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Repositories
{
    public class WarehouseMockRepository
    {

        private readonly List<Warehouse> _warehouses;

        public WarehouseMockRepository()
        {
            _warehouses = new List<Warehouse>
            {
                new Warehouse
            {
                Id = 1,
                Name = "Main Distribution Center",
                Address = "1250 Logistics Parkway",
                City = "Chicago",
                Country = "USA",
                Capacity = 1000
            },
            new Warehouse
            {
                Id = 2,
                Name = "Eastern Fulfillment Hub",
                Address = "840 Industrial Avenue",
                City = "Columbus",
                Country = "USA",
                Capacity = 750
            },
            new Warehouse
            {
                Id = 3,
                Name = "Western Logistics Center",
                Address = "620 Commerce Drive",
                City = "Phoenix",
                Country = "USA",
                Capacity = 500
            }
            };
        }

        public IReadOnlyList<Warehouse> GetAll()
        {
            return _warehouses;
           
        }

        public Warehouse? GetById(int id)
        {
            return _warehouses.FirstOrDefault(w => w.Id == id);

        }
    }
}
