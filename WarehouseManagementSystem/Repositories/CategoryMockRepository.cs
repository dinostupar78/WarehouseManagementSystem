using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public class CategoryMockRepository
    {
        private readonly List<Category> _categories;

        public CategoryMockRepository()
        {
            _categories = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic devices and accessories"
                },
                new Category
                {
                    Id = 2,
                    Name = "Furniture",
                    Description = "Warehouse furniture and storage solutions"
                },
                new Category
                {
                    Id = 3,
                    Name = "Office Supplies",
                    Description = "General office supplies and equipment"
                }
            };
        }

        public IReadOnlyList<Category> GetAll()
        {
            return _categories;
        }

        public Category GetById(int id)
        {
            return _categories.FirstOrDefault(c => c.Id == id);
        }
    }
}