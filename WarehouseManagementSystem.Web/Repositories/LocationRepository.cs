using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class LocationRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public LocationRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public IReadOnlyList<Location> GetAll()
        {
            return _db.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .Include(l => l.Inventories)
                .ToList();

        }

        public int GetTotalCount()
        {
            return _db.Locations.Count();
        }

        public int GetZoneCount()
        {
            return _db.Locations
                .Where(l => !string.IsNullOrWhiteSpace(l.Zone))
                .Select(l => l.Zone)
                .Distinct()
                .Count();
        }

        public int GetLinkedWarehouseCount()
        {
            return _db.Locations
                .Select(l => l.WarehouseId)
                .Distinct()
                .Count();
        }

        public Location? GetById(int id)
        {
            return _db.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .Include(l => l.Inventories)
                .FirstOrDefault(l => l.Id == id);
        }

        public void Add(Location location)
        {
            _db.Locations.Add(location);
            _db.SaveChanges();
        }

        public void Update(Location location)
        {
            _db.Locations.Update(location);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var location = _db.Locations.Find(id);
            if (location != null)
            {
                _db.Locations.Remove(location);
                _db.SaveChanges();
            }
        }

        public IReadOnlyList<Location> Search(string? term)
        {
            var query = _db.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .Include(l => l.Inventories)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();

                query = query.Where(l =>
                    l.Code.ToLower().Contains(term) ||
                    l.Zone.ToLower().Contains(term) ||
                    l.ShelfNumber.ToString().Contains(term) ||
                    l.Warehouse.Name.ToLower().Contains(term) ||
                    l.Warehouse.City.ToLower().Contains(term));
            }

            return query.ToList();
        }

        public IReadOnlyList<Location> GetByZone(string zone)
        {
            return _db.Locations
                .Include(l => l.Warehouse)
                .Where(l => l.Zone.ToLower() == zone.ToLower())
                .OrderBy(l => l.ShelfNumber)
                .ThenBy(l => l.Code)
                .ToList();
        }

        public IReadOnlyList<Location> GetByWarehouse(int warehouseId)
        {
            return _db.Locations
                .Include(l => l.Warehouse)
                .Where(l => l.WarehouseId == warehouseId)
                .OrderBy(l => l.Zone)
                .ThenBy(l => l.ShelfNumber)
                .ToList();
        }

        public IReadOnlyList<Location> GetShelfAbove(int shelfNumber)
        {
            return _db.Locations
                .Include(l => l.Warehouse)
                .Where(l => l.ShelfNumber >= shelfNumber)
                .OrderBy(l => l.ShelfNumber)
                .ThenBy(l => l.Code)
                .ToList();
        }

        public bool WarehouseExists(int warehouseId)
        {
            return _db.Warehouses.AsNoTracking().Any(w => w.Id == warehouseId);
        }

    }
}
