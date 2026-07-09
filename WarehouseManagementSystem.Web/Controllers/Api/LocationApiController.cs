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
        private readonly ILogger<LocationApiController> _logger;

        public LocationApiController(WarehouseManagementSystemDbContext dbContext, ILogger<LocationApiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
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
                _logger.LogWarning("API location lookup failed for ID {LocationId}", id);
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
                _logger.LogWarning("API location create rejected because Warehouse {WarehouseId} does not exist", dto.WarehouseId);
                return BadRequest("Selected warehouse does not exist.");
            }

            var location = ApiMapper.ToEntity(dto);

            _dbContext.Locations.Add(location);
            _dbContext.SaveChanges();

            var createdLocation = _dbContext.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .First(l => l.Id == location.Id);

            _logger.LogInformation("API created Location {LocationId} ({LocationCode})", location.Id, location.Code);

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
                _logger.LogWarning("API location update failed because ID {LocationId} was not found", id);
                return NotFound();
            }

            var warehouseExists = _dbContext.Warehouses
                .AsNoTracking()
                .Any(w => w.Id == dto.WarehouseId);

            if (!warehouseExists)
            {
                _logger.LogWarning("API location update rejected for Location {LocationId} because Warehouse {WarehouseId} does not exist", id, dto.WarehouseId);
                return BadRequest("Selected warehouse does not exist.");
            }

            ApiMapper.UpdateEntity(location, dto);

            _dbContext.SaveChanges();

            var updatedLocation = _dbContext.Locations
                .AsNoTracking()
                .Include(l => l.Warehouse)
                .First(l => l.Id == id);

            _logger.LogInformation("API updated Location {LocationId} ({LocationCode})", location.Id, location.Code);

            return Ok(ApiMapper.ToDto(updatedLocation));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var location = _dbContext.Locations
                .FirstOrDefault(l => l.Id == id);

            if (location == null)
            {
                _logger.LogWarning("API location delete failed because ID {LocationId} was not found", id);
                return NotFound();
            }

            _dbContext.Locations.Remove(location);
            _dbContext.SaveChanges();

            _logger.LogInformation("API deleted Location {LocationId} ({LocationCode})", location.Id, location.Code);

            return NoContent();
        }

    }
}
