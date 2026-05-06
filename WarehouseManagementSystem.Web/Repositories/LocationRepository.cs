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

        public Location? GetById(int id)
        {
            return _db.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .Include(l => l.Inventories)
                .FirstOrDefault(l => l.Id == id);
        }
    }
}