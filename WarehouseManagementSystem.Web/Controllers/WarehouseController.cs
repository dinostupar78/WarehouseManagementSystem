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
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _warehouseRepository.Update(warehouse);
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
                TempData["DeleteError"] = "Warehouse cannot be deleted because it has related purchase orders.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _warehouseRepository.Delete(id);
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
            var warehouses = _warehouseRepository.GetAll()
            .Where(w => w.City == city)
            .ToList();

            ViewBag.City = city;
            return View(warehouses);
        }

        [HttpGet("capacity-above/{minCapacity:int}")]
        public IActionResult CapacityAbove(int minCapacity)
        {
            var warehouses = _warehouseRepository.GetAll()
            .Where(w => w.Capacity > minCapacity)
            .ToList();

            ViewBag.MinCapacity = minCapacity;
            return View(warehouses);
        }
    }
}
