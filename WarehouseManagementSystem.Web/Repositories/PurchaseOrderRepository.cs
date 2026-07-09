using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class PurchaseOrderRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public PurchaseOrderRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public IReadOnlyList<PurchaseOrder> GetAll()
        {
            return _db.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Product)
                .ToList();
        }

        public int GetTotalCount()
        {
            return _db.PurchaseOrders.Count();
        }

        public int GetActiveOrderCount()
        {
            return _db.PurchaseOrders.Count(po =>
                po.Status != OrderStatus.Delivered &&
                po.Status != OrderStatus.Cancelled);
        }

        public decimal GetTotalOrderValue()
        {
            return _db.PurchaseOrders.Sum(po => po.TotalAmount);
        }

        public PurchaseOrder? GetById(int id)
        {
            return _db.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Product)
                .FirstOrDefault(po => po.Id == id);
        }

        public void Add(PurchaseOrder purchaseOrder)
        {
            _db.PurchaseOrders.Add(purchaseOrder);
            _db.SaveChanges();
        }

        public int GetNextOrderNumber()
        {
            var currentMax = _db.PurchaseOrders
                .AsNoTracking()
                .Select(po => (int?)po.OrderNumber)
                .Max() ?? 0;

            return currentMax + 1;
        }

        public void Update(PurchaseOrder purchaseOrder)
        {
            _db.PurchaseOrders.Update(purchaseOrder);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var purchaseOrder = _db.PurchaseOrders.Find(id);
            if (purchaseOrder != null)
            {
                _db.PurchaseOrders.Remove(purchaseOrder);
                _db.SaveChanges();
            }
        }

        public IReadOnlyList<PurchaseOrder> Search(string? term)
        {
            var query = _db.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Product)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();

                query = query.Where(po =>
                    po.OrderNumber.ToString().Contains(term) ||
                    po.TotalAmount.ToString().Contains(term) ||
                    po.Status.ToString().ToLower().Contains(term) ||
                    po.OrderDate.ToString("yyyy-MM-dd HH:mm").Contains(term) ||
                    po.ExpectedDeliveryDate.ToString("yyyy-MM-dd HH:mm").Contains(term) ||
                    (po.Supplier?.Name?.ToLower().Contains(term) ?? false) ||
                    (po.Warehouse?.Name?.ToLower().Contains(term) ?? false));
            }

            return query.ToList();
        }

        public IReadOnlyList<PurchaseOrder> GetBySupplier(int supplierId)
        {
            return _db.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Where(po => po.SupplierId == supplierId)
                .OrderByDescending(po => po.OrderDate)
                .ToList();
        }

        public IReadOnlyList<PurchaseOrder> GetOverdue()
        {
            var now = DateTime.Now;

            return _db.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Where(po =>
                    po.ExpectedDeliveryDate < now &&
                    po.Status != OrderStatus.Delivered &&
                    po.Status != OrderStatus.Cancelled)
                .OrderBy(po => po.ExpectedDeliveryDate)
                .ToList();
        }

        public IReadOnlyList<PurchaseOrder> GetByStatus(OrderStatus status)
        {
            return _db.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Where(po => po.Status == status)
                .OrderByDescending(po => po.OrderDate)
                .ToList();
        }

        public bool SupplierExists(int supplierId)
        {
            return _db.Suppliers.AsNoTracking().Any(s => s.Id == supplierId);
        }

        public bool WarehouseExists(int warehouseId)
        {
            return _db.Warehouses.AsNoTracking().Any(w => w.Id == warehouseId);
        }

    }
}
