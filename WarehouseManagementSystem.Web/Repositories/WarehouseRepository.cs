using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.Web.Repositories
{
    public class WarehouseRepository
    {
        private readonly WarehouseManagementSystemDbContext _db;

        public WarehouseRepository(WarehouseManagementSystemDbContext db)
        {
            _db = db;
        }

        public bool HasPurchaseOrders(int id)
        {
            return _db.PurchaseOrders.AsNoTracking().Any(po => po.WarehouseId == id);
        }

        public IReadOnlyList<Warehouse> GetAll()
        { 
            return _db.Warehouses
                .AsNoTracking()
                .Include(w => w.Locations)
                .ToList();
        }

        public Warehouse? GetById(int id)
        {
            return _db.Warehouses
                .AsNoTracking()
                .Include(w => w.Locations)
                .FirstOrDefault(w => w.Id == id);
        }

        public void Add(Warehouse warehouse)
        {
            _db.Warehouses.Add(warehouse);
            _db.SaveChanges();
        }

        public void Update(Warehouse warehouse)
        {
            _db.Warehouses.Update(warehouse);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var warehouse = _db.Warehouses.Find(id);
            if (warehouse != null)
            {
                _db.Warehouses.Remove(warehouse);
                _db.SaveChanges();
            }
        }

        public IReadOnlyList<Warehouse> Search(string? term)
        {
            var query = _db.Warehouses
                .AsNoTracking()
                .Include(w => w.Locations)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLower();

                query = query.Where(w =>
                    w.Name.ToLower().Contains(term) ||
                    w.Address.ToLower().Contains(term) ||
                    w.City.ToLower().Contains(term) ||
                    w.Country.ToLower().Contains(term) ||
                    w.Capacity.ToString().Contains(term));
            }

            return query.ToList();
        }

    }
}
