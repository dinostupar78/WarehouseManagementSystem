using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/warehouses")]
    public class WarehouseApiController : ControllerBase
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;
        private readonly ILogger<WarehouseApiController> _logger;

        public WarehouseApiController(WarehouseManagementSystemDbContext dbContext, ILogger<WarehouseApiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<WarehouseDto>> Get([FromQuery] string? query)
        {
            var warehousesQuery = _dbContext.Warehouses
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();

                warehousesQuery = warehousesQuery.Where(w =>
                    w.Name.ToLower().Contains(query) ||
                    w.Address.ToLower().Contains(query) ||
                    w.City.ToLower().Contains(query) ||
                    w.Country.ToLower().Contains(query) ||
                    w.Capacity.ToString().Contains(query));
                }

            var warehouses = warehousesQuery
                .ToList()
                .Select(ApiMapper.ToDto)
                .ToList();

            return Ok(warehouses);

        }

        [HttpGet("{id:int}")]
        public ActionResult<CategoryDto> Get(int id)
        {
            var warehouse = _dbContext.Warehouses
                .AsNoTracking()
                .FirstOrDefault(w => w.Id == id);

            if (warehouse == null)
            {
                _logger.LogWarning("API warehouse lookup failed for ID {WarehouseId}", id);
                return NotFound();
            }

            return Ok(ApiMapper.ToDto(warehouse));
        }

        [HttpPost]
        public ActionResult<WarehouseDto> Post([FromBody] WarehouseCreateDto dto)
        {
            var warehouse = ApiMapper.ToEntity(dto);

            this._dbContext.Warehouses.Add(warehouse);
            this._dbContext.SaveChanges();

            _logger.LogInformation("API created Warehouse {WarehouseId} ({WarehouseName})", warehouse.Id, warehouse.Name);

            return CreatedAtAction(
                nameof(Get),
                new { id = warehouse.Id },
                ApiMapper.ToDto(warehouse));
        }

        [HttpPut("{id:int}")]
        public ActionResult<WarehouseDto> Put(int id, [FromBody] WarehouseUpdateDto dto)
        {

            var warehouse = _dbContext.Warehouses
               .FirstOrDefault(w => w.Id == id);

            if (warehouse == null)
            {
                _logger.LogWarning("API warehouse update failed because ID {WarehouseId} was not found", id);
                return NotFound();
            }

            ApiMapper.UpdateEntity(warehouse, dto);

            this._dbContext.SaveChanges();

            _logger.LogInformation("API updated Warehouse {WarehouseId} ({WarehouseName})", warehouse.Id, warehouse.Name);

            return Ok(ApiMapper.ToDto(warehouse));

        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var warehouse = _dbContext.Warehouses
               .FirstOrDefault(w => w.Id == id);

            if (warehouse == null)
            {
                _logger.LogWarning("API warehouse delete failed because ID {WarehouseId} was not found", id);
                return NotFound();
            }

            var hasPurchaseOrders = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Any(po => po.WarehouseId == id);

            if (hasPurchaseOrders)
            {
                _logger.LogWarning("API blocked delete for Warehouse {WarehouseId} because related purchase orders exist", id);
                return Conflict("Warehouse cannot be deleted because it has related purchase orders.");
            }

            _dbContext.Warehouses.Remove(warehouse);
            _dbContext.SaveChanges();

            _logger.LogInformation("API deleted Warehouse {WarehouseId} ({WarehouseName})", warehouse.Id, warehouse.Name);

            return NoContent();
        }
    }
}
