using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Repositories
{
    public class ProductMockRepository
    {
        private readonly List<Product> _products;

        public ProductMockRepository(CategoryMockRepository categoryRepository)
        {
            var categoriesById = categoryRepository.GetAll().ToDictionary(c => c.Id);

            _products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Handheld Barcode Scanner",
                    Description = "Portable barcode scanner designed for fast and accurate inventory processing",
                    Price = 120.00m,
                    Weight = 0.35m,
                    ProductReceivedAt = new DateTime(2026, 01, 06),
                    CategoryId = 3
                },
                new Product
                {
                    Id = 2,
                    Name = "Industrial Label Printer",
                    Description = "High-speed thermal printer for warehouse and shipping labels",
                    Price = 230m,
                    Weight = 2.10m,
                    ProductReceivedAt = new DateTime(2026, 02, 15),
                    CategoryId = 3
                },
                new Product
                {
                    Id = 3,
                    Name = "Laptop",
                    Description = "15-inch laptop built for office productivity and warehouse administration",
                    Price = 1500m,
                    Weight = 1.5m,
                    ProductReceivedAt = new DateTime(2026, 02, 18),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 4,
                    Name = "Monitor",
                    Description = "Full HD monitor suitable for administrative and operational workstations",
                    Price = 300m,
                    Weight = 3.0m,
                    ProductReceivedAt = new DateTime(2026, 03, 07),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 5,
                    Name = "Ergonomic Office Chair",
                    Description = "Adjustable office chair designed for long-duration seated work",
                    Price = 250m,
                    Weight = 15m,
                    ProductReceivedAt = new DateTime(2026, 03, 18),
                    CategoryId = 2
                },
                new Product
                {
                    Id = 6,
                    Name = "Workstation Desk",
                    Description = "Durable desk with ample surface area for office and warehouse coordination tasks",
                    Price = 400m,
                    Weight = 30m,
                    ProductReceivedAt = new DateTime(2026, 03, 13),
                    CategoryId = 2
                }
            };

            foreach(var product in _products)
            {
                if (!categoriesById.TryGetValue(product.CategoryId, out var category))
                {
                    throw new InvalidOperationException($"CategoryId {product.CategoryId} does not exist in category seed data.");
                }

                product.Category = category;
                category.Products.Add(product);
            }

        }

        public IReadOnlyList<Product> GetAll() 
        {
            return _products;
        }

        public Product? GetById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);

        }
    }
}
