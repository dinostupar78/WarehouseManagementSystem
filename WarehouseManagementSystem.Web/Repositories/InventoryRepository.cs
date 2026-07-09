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

        public int GetTotalCount()
        {
            return _db.Inventories.Count();
        }

        public int GetTotalStockUnits()
        {
            return _db.Inventories.Sum(i => i.Quantity);
        }

        public int GetLowStockCount(int threshold = 10)
        {
            return _db.Inventories.Count(i => i.Quantity <= threshold);
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

        public IReadOnlyList<Inventory> GetLowStock(int threshold)
        {
            return _db.Inventories
                .Include(i => i.Product)
                .Include(i => i.Location)
                    .ThenInclude(l => l.Warehouse)
                .Where(i => i.Quantity <= threshold)
                .OrderBy(i => i.Quantity)
                .ToList();
        }

        public IReadOnlyList<Inventory> GetByLocation(int locationId)
        {
            return _db.Inventories
                .Include(i => i.Product)
                .Include(i => i.Location)
                    .ThenInclude(l => l.Warehouse)
                .Where(i => i.LocationId == locationId)
                .OrderBy(i => i.Product.Name)
                .ToList();
        }

        public IReadOnlyList<Inventory> GetByProduct(int productId)
        {
            return _db.Inventories
                .Include(i => i.Product)
                .Include(i => i.Location)
                    .ThenInclude(l => l.Warehouse)
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.Location.Code)
                .ToList();
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
