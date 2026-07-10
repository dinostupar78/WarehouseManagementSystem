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

        public int GetTotalCount()
        {
            return _db.Products.Count();
        }

        public decimal GetTotalCatalogValue()
        {
            return _db.Products.Sum(p => p.Price);
        }

        public Product? GetHighestValueProduct()
        {
            return _db.Products
                .AsNoTracking()
                .OrderByDescending(p => p.Price)
                .FirstOrDefault();
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

        public void Add(Product product)
        {
            _db.Products.Add(product);
            _db.SaveChanges();
        }

        public void Update(Product product)
        {
            _db.Products.Update(product);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var product = _db.Products.Find(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                _db.SaveChanges();
            }
        }

        public bool CategoryExists(int categoryId)
        {
            return _db.Categories.AsNoTracking().Any(c => c.Id == categoryId);
        }

        public IReadOnlyList<Product> Search(string? term)
        {
            var query = _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Inventories)
                .Include(p => p.PurchaseOrderItems)
                .AsEnumerable();


            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();

                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term) ||
                    p.Price.ToString().Contains(term) ||
                    p.Weight.ToString().Contains(term) ||
                    p.Category.Name.ToLower().Contains(term));
            }

            return query.ToList();
        }

        public IReadOnlyList<Product> GetPriceAbove(decimal minPrice)
        {
            return _db.Products
                .Include(p => p.Category)
                .Where(p => p.Price >= minPrice)
                .OrderByDescending(p => p.Price)
                .ToList();
        }

        public IReadOnlyList<Product> GetByCategory(int categoryId)
        {
            return _db.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Name)
                .ToList();
        }

        public IReadOnlyList<Product> GetWeightAbove(decimal minWeight)
        {
            return _db.Products
                .Include(p => p.Category)
                .Where(p => p.Weight >= minWeight)
                .OrderByDescending(p => p.Weight)
                .ToList();
        }

        public IReadOnlyList<Product> GetByWeightRange(decimal minWeight, decimal maxWeight, int excludedProductId)
        {
            return _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Id != excludedProductId && p.Weight >= minWeight && p.Weight <= maxWeight)
                .OrderBy(p => p.Weight)
                .ToList();
        }

        public bool HasPurchaseOrderItems(int id)
        {
            return _db.PurchaseOrderItems.AsNoTracking().Any(poi => poi.ProductId == id);
        }
    }
}
