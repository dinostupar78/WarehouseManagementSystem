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
        private readonly ILogger<SupplierApiController> _logger;

        public SupplierApiController(WarehouseManagementSystemDbContext dbContext, ILogger<SupplierApiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
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
                _logger.LogWarning("API supplier lookup failed for ID {SupplierId}", id);
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

            _logger.LogInformation("API created Supplier {SupplierId} ({SupplierName})", supplier.Id, supplier.Name);

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
                _logger.LogWarning("API supplier update failed because ID {SupplierId} was not found", id);
                return NotFound();
            }

            ApiMapper.UpdateEntity(supplier, dto);

            _dbContext.SaveChanges();

            _logger.LogInformation("API updated Supplier {SupplierId} ({SupplierName})", supplier.Id, supplier.Name);

            return Ok(ApiMapper.ToDto(supplier));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var supplier = _dbContext.Suppliers
                .FirstOrDefault(s => s.Id == id);

            if (supplier == null)
            {
                _logger.LogWarning("API supplier delete failed because ID {SupplierId} was not found", id);
                return NotFound();
            }

            var hasPurchaseOrders = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Any(po => po.SupplierId == id);

            if (hasPurchaseOrders)
            {
                _logger.LogWarning("API blocked delete for Supplier {SupplierId} because related purchase orders exist", id);
                return Conflict("Supplier cannot be deleted because it has related purchase orders.");
            }

            _dbContext.Suppliers.Remove(supplier);
            _dbContext.SaveChanges();

            _logger.LogInformation("API deleted Supplier {SupplierId} ({SupplierName})", supplier.Id, supplier.Name);

            return NoContent();
        }
    }
}
