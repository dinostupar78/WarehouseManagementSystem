using WarehouseManagementSystem.Models;
namespace WarehouseManagementSystem.Repositories
{
    public interface IWarehouseRepository
    {
        IReadOnlyList<Warehouse> GetAll();
            Warehouse GetById(int id);

    }
}
