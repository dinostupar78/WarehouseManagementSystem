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

        public int GetTotalCount()
        {
            return _db.PurchaseOrderItems.Count();
        }

        public int GetTotalQuantity()
        {
            return _db.PurchaseOrderItems.Sum(poi => poi.Quantity);
        }

        public decimal GetTotalItemValue()
        {
            return _db.PurchaseOrderItems.Sum(poi => poi.Quantity * poi.UnitPrice);
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

        public IReadOnlyList<PurchaseOrderItem> GetByPurchaseOrder(int purchaseOrderId)
        {
            return _db.PurchaseOrderItems
                .Include(poi => poi.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(poi => poi.Product)
                .Where(poi => poi.PurchaseOrderId == purchaseOrderId)
                .OrderBy(poi => poi.Product.Name)
                .ToList();
        }

        public IReadOnlyList<PurchaseOrderItem> GetByProduct(int productId)
        {
            return _db.PurchaseOrderItems
                .Include(poi => poi.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(poi => poi.Product)
                .Where(poi => poi.ProductId == productId)
                .OrderByDescending(poi => poi.PurchaseOrder.OrderDate)
                .ToList();
        }

        public IReadOnlyList<PurchaseOrderItem> GetPriceAbove(decimal minPrice)
        {
            return _db.PurchaseOrderItems
                .Include(poi => poi.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(poi => poi.Product)
                .Where(poi => poi.UnitPrice >= minPrice)
                .OrderByDescending(poi => poi.UnitPrice)
                .ToList();
        }

        public bool PurchaseOrderExists(int purchaseOrderId)
        {
            return _db.PurchaseOrders.AsNoTracking().Any(po => po.Id == purchaseOrderId);
        }

        public bool ProductExists(int productId)
        {
            return _db.Products.AsNoTracking().Any(p => p.Id == productId);
        }

    }
}
