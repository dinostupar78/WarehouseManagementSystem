using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class InventoryRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public InventoryRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public IReadOnlyList<Inventory> GetAll()
        {
            return _db.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Location)
                .ToList();

        }

        public Inventory? GetById(int id)
        {
            return _db.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Location)
                .FirstOrDefault(i => i.Id == id);
        }

        public void Add(Inventory inventory)
        {
            _db.Inventories.Add(inventory);
            _db.SaveChanges();
        }

        public void Update(Inventory inventory)
        {
            _db.Inventories.Update(inventory);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var inventory = _db.Inventories.Find(id);
            if (inventory != null)
            {
                _db.Inventories.Remove(inventory);
                _db.SaveChanges();
            }
        }

        public bool ProductExists(int productId)
        {
            return _db.Products.AsNoTracking().Any(p => p.Id == productId);
        }

        public bool LocationExists(int locationId)
        {
            return _db.Locations.AsNoTracking().Any(l => l.Id == locationId);
        }

        public IReadOnlyList<Inventory> Search(string? term)
        {
            var query = _db.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Location)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();

                query = query.Where(i =>
                    i.Quantity.ToString().Contains(term) ||
                    i.LastUpdated.ToString().Contains(term) ||
                    i.Product.Name.ToLower().Contains(term) ||
                    i.Location.Code.ToLower().Contains(term) ||
                    i.Location.Zone.ToLower().Contains(term));
            }
            
            return query.ToList();
        }
    }
}
