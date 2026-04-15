using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public interface ICategoryRepository
    {
        IReadOnlyList<Category> GetAll();
        Category GetById(int id);
    }
}
