using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public interface ILocationRepository
    {
        IReadOnlyList<Location> GetAll();
        Location GetById(int id);
    }
}
