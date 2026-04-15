using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public interface IPurchaseOrderRepository
    {
        IReadOnlyList<PurchaseOrder> GetAll();
        PurchaseOrder GetById(int id);
    }
}
