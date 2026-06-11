using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class SupplierRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public SupplierRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public bool HasPurchaseOrders(int id)
        {
            return _db.PurchaseOrders.AsNoTracking().Any(po => po.SupplierId == id);
        }

        public IReadOnlyList<Supplier> GetAll()
        { 
            return _db.Suppliers
                .AsNoTracking()
                .Include(s => s.PurchaseOrders)
                .ToList();
        }

        public Supplier? GetById(int id)
        {
            return _db.Suppliers
                .AsNoTracking()
                .Include(s => s.PurchaseOrders)
                .FirstOrDefault(s => s.Id == id);
        }

        public void Add(Supplier supplier)
        {
            _db.Suppliers.Add(supplier);
            _db.SaveChanges();
        }

        public void Update(Supplier supplier)
        {
            _db.Suppliers.Update(supplier);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var supplier = _db.Suppliers.Find(id);
            if (supplier != null)
            {
                _db.Suppliers.Remove(supplier);
                _db.SaveChanges();
            }
        }

        public IReadOnlyList<Supplier> Search(string? term)
        {
            var query = _db.Suppliers
                .AsNoTracking()
                .Include(s => s.PurchaseOrders)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();

                query = query.Where(s => 
                s.Name.ToLower().Contains(term) || 
                s.ContactPerson.ToLower().Contains(term) ||
                s.ContactEmail.ToLower().Contains(term) ||
                s.ContactPhone.ToLower().Contains(term) ||
                s.ContactAddress.ToLower().Contains(term));
            }

            return query.ToList();
        }    
    }
}
