using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Web.Repositories;
using WarehouseManagementSystem.Model;

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
        public IActionResult Index()
        {
            var warehouses = _warehouseRepository.GetAll();
            return View(warehouses);
        }

        [HttpGet("{id:int}")]
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
        public IActionResult Create() { 
            return View();
        }

        [HttpPost("create")]
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
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _warehouseRepository.Delete(id);
            TempData["ToastTitle"] = "Warehouse deleted";
            TempData["ToastMessage"] = "Warehouse was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("search")]
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
