using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class SupplierRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public SupplierRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public IReadOnlyList<Supplier> GetAll()
        { 
            return _db.Suppliers
                .AsNoTracking()
                .Include(s => s.PurchaseOrders)
                .ToList();
        }

        public Supplier? GetById(int id)
        {
            return _db.Suppliers
                .AsNoTracking()
                .Include(s => s.PurchaseOrders)
                .FirstOrDefault(s => s.Id == id);
        }
    }
}
