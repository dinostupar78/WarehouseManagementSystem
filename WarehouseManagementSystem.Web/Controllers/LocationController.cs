using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Model;
using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        public IActionResult Index()
        {
            var locations = _locationRepository.GetAll();

            ViewBag.TotalLocations = _locationRepository.GetTotalCount();
            ViewBag.TotalZones = _locationRepository.GetZoneCount();
            ViewBag.LinkedWarehouses = _locationRepository.GetLinkedWarehouseCount();

            return View(locations);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
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
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Location location)
        {
            ValidateLocationReferences(location);

            if (ModelState.IsValid)
            {
                _locationRepository.Add(location);

                _logger.LogInformation(
                    "User {User} created Location {LocationId} ({LocationCode})",
                    User.Identity?.Name ?? "Anonymous",
                    location.Id,
                    location.Code);

                TempData["ToastTitle"] = "Location created";
                TempData["ToastMessage"] = "Location was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        [HttpGet("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Edit(int id)
        {
            var location = _locationRepository.GetById(id);
            if (location == null)
            {
                _logger.LogWarning("Location not found for edit with ID: {LocationId}", id);
                return NotFound();
            }
            return View(location);
        }

        [HttpPost("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Location location)
        {
            ValidateLocationReferences(location);

            if (id != location.Id)
            {
                _logger.LogWarning("Location edit rejected because route ID {RouteId} does not match model ID {ModelId}", id, location.Id);
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _locationRepository.Update(location);

                _logger.LogInformation(
                    "User {User} updated Location {LocationId} ({LocationCode})",
                    User.Identity?.Name ?? "Anonymous",
                    location.Id,
                    location.Code);

                TempData["ToastTitle"] = "Location updated";
                TempData["ToastMessage"] = "Location was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(location);
        }

        [HttpGet("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var location = _locationRepository.GetById(id);
            _locationRepository.Delete(id);

            _logger.LogWarning(
                "User {User} deleted Location {LocationId} ({LocationCode})",
                User.Identity?.Name ?? "Anonymous",
                id,
                location?.Code ?? "Unknown");

            TempData["ToastTitle"] = "Location deleted";
            TempData["ToastMessage"] = "Location was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("search")]
        [AllowAnonymous]
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

        [HttpGet("by-zone")]
        public IActionResult ByZone(string zone)
        {
            if (string.IsNullOrWhiteSpace(zone))
            {
                return BadRequest();
            }

            var locations = _locationRepository.GetByZone(zone);

            ViewBag.Zone = zone;

            _logger.LogInformation(
                "User {User} viewed locations in Zone {Zone}",
                User.Identity?.Name ?? "Anonymous",
                zone);

            return View(locations);
        }

        [HttpGet("by-warehouse/{warehouseId:int}")]
        public IActionResult ByWarehouse(int warehouseId)
        {
            var locations = _locationRepository.GetByWarehouse(warehouseId);

            ViewBag.WarehouseId = warehouseId;
            ViewBag.WarehouseName = locations.FirstOrDefault()?.Warehouse?.Name ?? $"Warehouse {warehouseId}";

            _logger.LogInformation(
                "User {User} viewed locations for Warehouse {WarehouseId}",
                User.Identity?.Name ?? "Anonymous",
                warehouseId);

            return View(locations);
        }

        [HttpGet("shelf-above")]
        public IActionResult ShelfAbove(int shelfNumber)
        {
            var locations = _locationRepository.GetShelfAbove(shelfNumber);

            ViewBag.ShelfNumber = shelfNumber;

            _logger.LogInformation(
                "User {User} viewed locations with shelf number above {ShelfNumber}",
                User.Identity?.Name ?? "Anonymous",
                shelfNumber);

            return View(locations);
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
