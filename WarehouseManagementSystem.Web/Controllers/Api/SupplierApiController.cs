using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/suppliers")]
    public class SupplierApiController : ControllerBase
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;

        public SupplierApiController(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SupplierDto>> Get([FromQuery] string? query)
        {
            var suppliersQuery = _dbContext.Suppliers
                .AsNoTracking()
                .Include(s => s.PurchaseOrders)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();

                suppliersQuery = suppliersQuery.Where(s =>
                    s.Name.ToLower().Contains(query) ||
                    s.ContactPerson.ToLower().Contains(query) ||
                    s.ContactEmail.ToLower().Contains(query) ||
                    s.ContactPhone.ToLower().Contains(query) ||
                    s.ContactAddress.ToLower().Contains(query));
            }

            var suppliers = suppliersQuery
                .ToList()
                .Select(ApiMapper.ToDto)
                .ToList();

            return Ok(suppliers);
        }

        [HttpGet("{id:int}")]
        public ActionResult<SupplierDto> Get(int id)
        {
            var supplier = _dbContext.Suppliers
                .AsNoTracking()
                .Include(s => s.PurchaseOrders)
                .FirstOrDefault(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound();
            }

            return Ok(ApiMapper.ToDto(supplier));
        }

        [HttpPost]
        public ActionResult<SupplierDto> Post([FromBody] SupplierCreateDto dto)
        {
            var supplier = ApiMapper.ToEntity(dto);

            this._dbContext.Suppliers.Add(supplier);
            this._dbContext.SaveChanges();

            return CreatedAtAction(
                nameof(Get),
                new { id = supplier.Id },
                ApiMapper.ToDto(supplier));
        }

        [HttpPut("{id:int}")]
        public ActionResult<SupplierDto> Put(int id, [FromBody] SupplierUpdateDto dto)
        {
            var supplier = _dbContext.Suppliers
                .FirstOrDefault(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound();
            }

            ApiMapper.UpdateEntity(supplier, dto);

            _dbContext.SaveChanges();

            return Ok(ApiMapper.ToDto(supplier));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var supplier = _dbContext.Suppliers
                .FirstOrDefault(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound();
            }

            var hasPurchaseOrders = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Any(po => po.SupplierId == id);

            if (hasPurchaseOrders)
            {
                return Conflict("Supplier cannot be deleted because it has related purchase orders.");
            }

            _dbContext.Suppliers.Remove(supplier);
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
