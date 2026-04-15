using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public interface IInventoryRepository
    {
        IReadOnlyList<Inventory> GetAll();
        Inventory GetById(int id);
    
    }
}
