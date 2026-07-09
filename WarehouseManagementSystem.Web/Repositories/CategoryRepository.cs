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

        public int GetTotalCount()
        {
            return _db.Categories.Count();
        }

        public int GetDocumentedCount()
        {
            return _db.Categories.Count(c => !string.IsNullOrWhiteSpace(c.Description));
        }

        public int GetAverageDescriptionLength()
        {
            var descriptionLengths = _db.Categories
                .AsNoTracking()
                .Where(c => !string.IsNullOrWhiteSpace(c.Description))
                .Select(c => c.Description.Length)
                .ToList();

            return descriptionLengths.Any()
                ? (int)Math.Round(descriptionLengths.Average())
                : 0;
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

        public IReadOnlyList<Category> GetWithProducts()
        {
            return _db.Categories
                .Include(c => c.Products)
                .Where(c => c.Products.Any())
                .OrderBy(c => c.Name)
                .ToList();
        }

        public IReadOnlyList<Category> GetWithoutProducts()
        {
            return _db.Categories
                .Include(c => c.Products)
                .Where(c => !c.Products.Any())
                .OrderBy(c => c.Name)
                .ToList();
        }

        public IReadOnlyList<Category> GetByDescriptionKeyword(string keyword)
        {
            return _db.Categories
                .Include(c => c.Products)
                .Where(c => c.Description != null &&
                            c.Description.ToLower().Contains(keyword.ToLower()))
                .OrderBy(c => c.Name)
                .ToList();
        }
    }
}
