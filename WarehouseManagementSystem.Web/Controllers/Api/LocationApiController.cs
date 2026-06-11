using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/locations")]
    public class LocationApiController : ControllerBase
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;

        public LocationApiController(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<LocationDto>> Get([FromQuery] string? query)
        {
            var locationsQuery = _dbContext.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();

                locationsQuery = locationsQuery.Where(l =>
                    l.Code.ToLower().Contains(query) ||
                    l.Zone.ToLower().Contains(query) ||
                    l.ShelfNumber.ToString().Contains(query) ||
                    l.Warehouse.Name.ToLower().Contains(query));
            }

            var locations = locationsQuery
                .ToList()
                .Select(ApiMapper.ToDto)
                .ToList();

            return Ok(locations);
        }

        [HttpGet("{id:int}")]
        public ActionResult<LocationDto> Get(int id)
        {
            var location = _dbContext.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .FirstOrDefault(l => l.Id == id);

            if (location == null)
            {
                return NotFound();
            }

            return Ok(ApiMapper.ToDto(location));
        }

        [HttpPost]
        public ActionResult<LocationDto> Post([FromBody] LocationCreateDto dto)
        {
            var warehouseExists = _dbContext.Warehouses
                .AsNoTracking()
                .Any(w => w.Id == dto.WarehouseId);

            if (!warehouseExists)
            {
                return BadRequest("Selected warehouse does not exist.");
            }

            var location = ApiMapper.ToEntity(dto);

            _dbContext.Locations.Add(location);
            _dbContext.SaveChanges();

            var createdLocation = _dbContext.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .First(l => l.Id == location.Id);

            return CreatedAtAction(
                nameof(Get),
                new { id = location.Id },
                ApiMapper.ToDto(createdLocation));
        }

        [HttpPut("{id:int}")]
        public ActionResult<LocationDto> Put(int id, [FromBody] LocationUpdateDto dto)
        {
            var location = _dbContext.Locations
                .FirstOrDefault(l => l.Id == id);

            if (location == null)
            {
                return NotFound();
            }

            var warehouseExists = _dbContext.Warehouses
                .AsNoTracking()
                .Any(w => w.Id == dto.WarehouseId);

            if (!warehouseExists)
            {
                return BadRequest("Selected warehouse does not exist.");
            }

            ApiMapper.UpdateEntity(location, dto);

            _dbContext.SaveChanges();

            var updatedLocation = _dbContext.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .First(l => l.Id == id);

            return Ok(ApiMapper.ToDto(updatedLocation));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var location = _dbContext.Locations
                .FirstOrDefault(l => l.Id == id);

            if (location == null)
            {
                return NotFound();
            }

            _dbContext.Locations.Remove(location);
            _dbContext.SaveChanges();

            return NoContent();
        }

    }
}
