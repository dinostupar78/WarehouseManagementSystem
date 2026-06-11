using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/inventories")]
    public class InventoryApiController : ControllerBase
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;

        public InventoryApiController(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<InventoryDto>> Get([FromQuery] string? query)
        {
            var inventoriesQuery = _dbContext.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Location)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();

                inventoriesQuery = inventoriesQuery.Where(i =>
                    i.Quantity.ToString().Contains(query) ||
                    i.Product.Name.ToLower().Contains(query) ||
                    i.Location.Code.ToLower().Contains(query) ||
                    i.Location.Zone.ToLower().Contains(query));
            }

            var inventories = inventoriesQuery
                .ToList()
                .Select(ApiMapper.ToDto)
                .ToList();

            return Ok(inventories);
        }

        [HttpGet("{id:int}")]
        public ActionResult<InventoryDto> Get(int id)
        {
            var inventory = _dbContext.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Location)
                .FirstOrDefault(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            return Ok(ApiMapper.ToDto(inventory));
        }

        [HttpPost]
        public ActionResult<InventoryDto> Post([FromBody] InventoryCreateDto dto)
        {
            var productExists = _dbContext.Products
                .AsNoTracking()
                .Any(p => p.Id == dto.ProductId);

            if (!productExists)
            {
                return BadRequest("Selected product does not exist.");
            }

            var locationExists = _dbContext.Locations
                .AsNoTracking()
                .Any(l => l.Id == dto.LocationId);

            if (!locationExists)
            {
                return BadRequest("Selected location does not exist.");
            }

            var inventory = ApiMapper.ToEntity(dto);

            this._dbContext.Inventories.Add(inventory);
            this._dbContext.SaveChanges();

            var createdInventory = _dbContext.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Location)
                .First(i => i.Id == inventory.Id);

            return CreatedAtAction(
                nameof(Get),
                new { id = inventory.Id },
                ApiMapper.ToDto(createdInventory));
        }

        [HttpPut("{id:int}")]
        public ActionResult<InventoryDto> Put(int id, [FromBody] InventoryUpdateDto dto)
        {
            var inventory = _dbContext.Inventories
                .FirstOrDefault(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            var productExists = _dbContext.Products
                .AsNoTracking()
                .Any(p => p.Id == dto.ProductId);

            if (!productExists)
            {
                return BadRequest("Selected product does not exist.");
            }

            var locationExists = _dbContext.Locations
                .AsNoTracking()
                .Any(l => l.Id == dto.LocationId);

            if (!locationExists)
            {
                return BadRequest("Selected location does not exist.");
            }

            ApiMapper.UpdateEntity(inventory, dto);

            this._dbContext.SaveChanges();

            var updatedInventory = _dbContext.Inventories
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Location)
                .First(i => i.Id == id);

            return Ok(ApiMapper.ToDto(updatedInventory));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var inventory = _dbContext.Inventories
                .FirstOrDefault(i => i.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            _dbContext.Inventories.Remove(inventory);
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
