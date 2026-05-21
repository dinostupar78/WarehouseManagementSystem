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

        public void Add(PurchaseOrderItem purchaseOrderItem)
        {
            _db.PurchaseOrderItems.Add(purchaseOrderItem);
            _db.SaveChanges();
        }

        public void Update(PurchaseOrderItem purchaseOrderItem)
        {
            _db.PurchaseOrderItems.Update(purchaseOrderItem);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var purchaseOrderItem = _db.PurchaseOrderItems.Find(id);
            if (purchaseOrderItem != null)
            {
                _db.PurchaseOrderItems.Remove(purchaseOrderItem);
                _db.SaveChanges();
            }
        }

        public bool PurchaseOrderExists(int purchaseOrderId)
        {
            return _db.PurchaseOrders.AsNoTracking().Any(po => po.Id == purchaseOrderId);
        }

        public bool ProductExists(int productId)
        {
            return _db.Products.AsNoTracking().Any(p => p.Id == productId);
        }

        public IReadOnlyList<PurchaseOrderItem> Search(string? term)
        {
            var query = _db.PurchaseOrderItems
                .AsNoTracking()
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();

                query = query.Where(poi =>
                    poi.Quantity.ToString().Contains(term) ||
                    poi.UnitPrice.ToString().Contains(term) ||
                    (poi.Product?.Name?.ToLower().Contains(term) ?? false) ||
                    (poi.Product?.Description?.ToLower().Contains(term) ?? false) ||
                    (poi.PurchaseOrder?.OrderNumber.ToString().Contains(term) ?? false) ||
                    (poi.PurchaseOrder?.Status.ToString().ToLower().Contains(term) ?? false));
            }

            return query.ToList();
        }
    }
}
