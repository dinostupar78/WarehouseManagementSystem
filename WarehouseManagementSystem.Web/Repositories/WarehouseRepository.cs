using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class WarehouseRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public WarehouseRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public IReadOnlyList<Warehouse> GetAll()
        { 
            return _db.Warehouses
                .AsNoTracking()
                .Include(w => w.Locations)
                .ToList();
        }

        public Warehouse? GetById(int id)
        {
            return _db.Warehouses
                .AsNoTracking()
                .Include(w => w.Locations)
                .FirstOrDefault(w => w.Id == id);
        }
    }
}
