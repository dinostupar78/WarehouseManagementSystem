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

        public WarehouseApiController(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
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
                return NotFound();
            }

            ApiMapper.UpdateEntity(warehouse, dto);

            this._dbContext.SaveChanges();

            return Ok(ApiMapper.ToDto(warehouse));

        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var warehouse = _dbContext.Warehouses
               .FirstOrDefault(w => w.Id == id);

            if (warehouse == null)
            {
                return NotFound();
            }

            var hasPurchaseOrders = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Any(po => po.WarehouseId == id);

            if (hasPurchaseOrders)
            {
                return Conflict("Warehouse cannot be deleted because it has related purchase orders.");
            }

            _dbContext.Warehouses.Remove(warehouse);
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
