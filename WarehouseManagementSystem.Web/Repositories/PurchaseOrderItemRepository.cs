using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class PurchaseOrderItemRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public PurchaseOrderItemRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public IReadOnlyList<PurchaseOrderItem> GetAll()
        {
            return _db.PurchaseOrderItems
                .AsNoTracking()
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .ToList();

        }

        public PurchaseOrderItem? GetById(int id)
        {
            return _db.PurchaseOrderItems
                .AsNoTracking()
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .FirstOrDefault(poi => poi.Id == id);
        }
    }
}
