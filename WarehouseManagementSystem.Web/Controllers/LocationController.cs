using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Web.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    [Route("locations")]
    public class LocationController : Controller
    {
        private readonly LocationRepository _locationRepository;
        private readonly ILogger<LocationController> _logger;

        public LocationController(LocationRepository locationRepository, ILogger<LocationController> logger)
        {
            _locationRepository = locationRepository;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var locations = _locationRepository.GetAll();
            return View(locations);
        }

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid location ID: {LocationId}", id);
                return BadRequest();
            }

            var location = _locationRepository.GetById(id);

            if (location == null)
            {
                _logger.LogWarning("Location not found with ID: {LocationId}", id);
                return NotFound();
            }

            return View(location);
        }
    }
}
