using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public interface IProductRepository
    {
            IReadOnlyList<Product> GetAll();
            Product GetById(int id);
    }
}
