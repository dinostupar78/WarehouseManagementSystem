using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public class SupplierMockRepository
    {
        private readonly List<Supplier> _suppliers;

        public SupplierMockRepository()
        {
            _suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Id = 1,
                    Name = "AutoID Systems",
                    ContactPerson = "Michael Carter",
                    ContactEmail = "michael.carter@autoidsystems.com",
                    ContactPhone = "+1-312-555-0142",
                    ContactAddress = "910 Industrial Park Road, Chicago, USA"
                },
                new Supplier
                {
                    Id = 2,
                    Name = "TechCore Solutions",
                    ContactPerson = "Laura Bennett",
                    ContactEmail = "laura.bennett@techcore.com",
                    ContactPhone = "+1-614-555-0187",
                    ContactAddress = "455 Technology Avenue, Columbus, USA"
                },
                new Supplier
                {
                    Id = 3,
                    Name = "Office Furnishings Group",
                    ContactPerson = "Daniel Foster",
                    ContactEmail = "daniel.foster@officefurnishings.com",
                    ContactPhone = "+1-602-555-0119",
                    ContactAddress = "220 Business Center Drive, Phoenix, USA"
                }
            };
        }

        public IReadOnlyList<Supplier> GetAll()
        {
            return _suppliers;
        }

        public Supplier GetById(int id)
        {
            return _suppliers.FirstOrDefault(s => s.Id == id);
        }
    }
}   