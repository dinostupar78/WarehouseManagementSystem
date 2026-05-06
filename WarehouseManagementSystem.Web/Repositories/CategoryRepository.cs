using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class CategoryRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public CategoryRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public IReadOnlyList<Category> GetAll()
        {
            return _db.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .ToList();

        }

        public Category? GetById(int id)
        {
            return _db.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);

        }
    }
}
