using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Web.Repositories;
using WarehouseManagementSystem.Model;
using Microsoft.AspNetCore.Authorization;

namespace WarehouseManagementSystem.Controllers
{
    [Route("warehouses")]
    public class WarehouseController : Controller
    {
        private readonly WarehouseRepository _warehouseRepository;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(WarehouseRepository warehouseRepository, ILogger<WarehouseController> logger)
        {
            _warehouseRepository = warehouseRepository;
            _logger = logger;
        }

        [HttpGet("")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var warehouses = _warehouseRepository.GetAll();

            ViewBag.TotalWarehouses = _warehouseRepository.GetTotalCount();
            ViewBag.TotalCapacity = _warehouseRepository.GetTotalCapacity();

            var largestWarehouse = _warehouseRepository.GetLargestWarehouse();
            ViewBag.LargestWarehouseName = largestWarehouse?.Name ?? "No warehouses";
            ViewBag.LargestWarehouseCapacity = largestWarehouse?.Capacity ?? 0;

            return View(warehouses);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid warehouse ID: {WarehouseId}", id);
                return BadRequest();
            }

            var warehouse = _warehouseRepository.GetById(id);

            if (warehouse == null)
            {
                _logger.LogWarning("Warehouse not found with ID: {WarehouseId}", id);
                return NotFound();

            }

            return View(warehouse);
        }

        [HttpGet("create")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Create() { 
            return View();
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                _warehouseRepository.Add(warehouse);

                _logger.LogInformation(
                    "User {User} created Warehouse {WarehouseId} ({WarehouseName})",
                    User.Identity?.Name ?? "Anonymous",
                    warehouse.Id,
                    warehouse.Name);

                TempData["ToastTitle"] = "Warehouse created";
                TempData["ToastMessage"] = "Warehouse was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        [HttpGet("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Edit(int id)
        {
            var warehouse = _warehouseRepository.GetById(id);
            if (warehouse == null)
            {
                _logger.LogWarning("Warehouse not found for edit with ID: {WarehouseId}", id);
                return NotFound();
            }
            return View(warehouse);
        }

        [HttpPost("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Warehouse warehouse)
        {
            if (id != warehouse.Id)
            {
                _logger.LogWarning("Warehouse edit rejected because route ID {RouteId} does not match model ID {ModelId}", id, warehouse.Id);
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _warehouseRepository.Update(warehouse);

                _logger.LogInformation(
                    "User {User} updated Warehouse {WarehouseId} ({WarehouseName})",
                    User.Identity?.Name ?? "Anonymous",
                    warehouse.Id,
                    warehouse.Name);

                TempData["ToastTitle"] = "Warehouse updated";
                TempData["ToastMessage"] = "Warehouse was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        [HttpGet("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid warehouse ID for delete: {WarehouseId}", id);
                return BadRequest();
            }

            var warehouse = _warehouseRepository.GetById(id);

            if (warehouse == null)
            {
                _logger.LogWarning("Warehouse not found for delete with ID: {WarehouseId}", id);
                return NotFound();
            }
            return View(warehouse);
        }

        [HttpPost("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (_warehouseRepository.HasPurchaseOrders(id))
            {
                _logger.LogWarning(
                    "User {User} tried to delete Warehouse {WarehouseId}, but delete was blocked because it has related purchase orders",
                    User.Identity?.Name ?? "Anonymous",
                    id);

                TempData["DeleteError"] = "Warehouse cannot be deleted because it has related purchase orders.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var warehouse = _warehouseRepository.GetById(id);
            _warehouseRepository.Delete(id);

            _logger.LogWarning(
                "User {User} deleted Warehouse {WarehouseId} ({WarehouseName})",
                User.Identity?.Name ?? "Anonymous",
                id,
                warehouse?.Name ?? "Unknown");

            TempData["ToastTitle"] = "Warehouse deleted";
            TempData["ToastMessage"] = "Warehouse was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var warehouses = _warehouseRepository.Search(term);
            return PartialView("_WarehouseListPartial", warehouses);
        }

        [HttpGet("autocomplete")]
        public IActionResult Autocomplete(string? term)
        {
            var warehouses = _warehouseRepository.Search(term)
                .Take(10)
                .Select(w => new
                {
                    id = w.Id,
                    text = w.Name,
                    subtitle = $"{w.City}, {w.Country}"
                })
                .ToList();

            return Json(warehouses);
        }

        [HttpGet("city/{city}")]
        public IActionResult FindByCity(string city)
        {
            var warehouses = _warehouseRepository.GetByCity(city);

            ViewBag.City = city;

            _logger.LogInformation(
                "User {User} viewed warehouses in city {City}",
                User.Identity?.Name ?? "Anonymous",
                city);

            return View(warehouses);
        }

        [HttpGet("country/{country}")]
        public IActionResult FindByCountry(string country)
        {
            var warehouses = _warehouseRepository.GetByCountry(country);

            ViewBag.Country = country;

            _logger.LogInformation(
                "User {User} viewed warehouses in country {Country}",
                User.Identity?.Name ?? "Anonymous",
                country);

            return View(warehouses);
        }

        [HttpGet("capacity-above/{capacity:int}")]
        public IActionResult CapacityAbove(int capacity)
        {
            var warehouses = _warehouseRepository.GetCapacityAbove(capacity);

            ViewBag.Capacity = capacity;

            _logger.LogInformation(
                "User {User} viewed warehouses with capacity above {Capacity}",
                User.Identity?.Name ?? "Anonymous",
                capacity);

            return View(warehouses);
        }


    }
}
