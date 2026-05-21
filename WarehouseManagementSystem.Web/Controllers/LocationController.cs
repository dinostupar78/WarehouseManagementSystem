using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Model;
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

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Location location)
        {
            ValidateLocationReferences(location);

            if (ModelState.IsValid)
            {
                _locationRepository.Add(location);
                TempData["ToastTitle"] = "Location created";
                TempData["ToastMessage"] = "Location was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var location = _locationRepository.GetById(id);
            if (location == null)
            {
                return NotFound();
            }
            return View(location);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Location location)
        {
            ValidateLocationReferences(location);

            if (id != location.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _locationRepository.Update(location);
                TempData["ToastTitle"] = "Location updated";
                TempData["ToastMessage"] = "Location was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(location);
        }

        [HttpGet("{id:int}/delete")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid location ID for delete: {LocationId}", id);
                return BadRequest();
            }

            var location = _locationRepository.GetById(id);

            if (location == null)
            {
                _logger.LogWarning("Location not found for delete with ID: {LocationId}", id);
                return NotFound();
            }
            return View(location);
        }

        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _locationRepository.Delete(id);
            TempData["ToastTitle"] = "Location deleted";
            TempData["ToastMessage"] = "Location was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("search")]
        public IActionResult Search(string? term)
        {
            var locations = _locationRepository.Search(term);
            return PartialView("_LocationListPartial", locations);
        }

        [HttpGet("autocomplete")]
        public IActionResult Autocomplete(string? term)
        {
            var locations = _locationRepository.Search(term)
                .Take(10)
                .Select(l => new { 
                    id = l.Id, 
                    text = l.Code,
                    subtitle = $"{l.Zone}, shelf {l.ShelfNumber} - {l.Warehouse?.Name ?? "Warehouse not assigned"}"
                })
                .ToList();

            return Json(locations);
        }

        private void ValidateLocationReferences(Location location)
        {
            if (location.WarehouseId > 0 && !_locationRepository.WarehouseExists(location.WarehouseId))
            {
                ModelState.AddModelError(nameof(Location.WarehouseId), "Selected warehouse does not exist.");
            }
        }
    }
}
