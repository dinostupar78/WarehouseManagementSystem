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
    }
}
