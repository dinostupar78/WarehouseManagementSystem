using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class ProductRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public ProductRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public IReadOnlyList<Product> GetAll()
        {
            return _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Inventories)
                .Include(p => p.PurchaseOrderItems)
                .ToList();
        }

        public Product? GetById(int id)
        {
            return _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Inventories)
                .Include(p => p.PurchaseOrderItems)
                .FirstOrDefault(p => p.Id == id);
        }
    }
}
