using Microsoft.EntityFrameworkCore;
using System.Linq;
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

        public bool HasProducts(int id)
        {
            return _db.Products.AsNoTracking().Any(p => p.CategoryId == id);
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

        public void Add(Category category)
        {
            _db.Categories.Add(category);
            _db.SaveChanges();
        }

        public void Update(Category category)
        {
            _db.Categories.Update(category);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var category = _db.Categories.Find(id);
            if (category != null)
            {
                _db.Categories.Remove(category);
                _db.SaveChanges();
            }
        }

        public IReadOnlyList<Category> Search(string? term)
        {
            var query = _db.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();

                query = query.Where(c => 
                c.Name.ToLower().Contains(term) ||
                c.Description.ToLower().Contains(term));
            }

            return query.ToList();
        }
    }
}
