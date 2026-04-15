using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public interface ISupplierRepository
    {
        IReadOnlyList<Supplier> GetAll();
        Supplier GetById(int id);
    }
}
