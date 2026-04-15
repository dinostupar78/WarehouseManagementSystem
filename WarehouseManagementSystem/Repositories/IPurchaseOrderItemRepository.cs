using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Repositories
{
    public interface IPurchaseOrderItemRepository
    {
        IReadOnlyList<PurchaseOrderItem> GetAll();
        PurchaseOrderItem GetById(int id);
    }
}
