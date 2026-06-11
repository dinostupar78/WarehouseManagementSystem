using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Model;
using Microsoft.AspNetCore.Authorization;
using WarehouseManagementSystem.Web.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    [Route("inventories")]
    public class InventoryController : Controller
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(InventoryRepository inventoryRepository, ILogger<InventoryController> logger)
        {
            _inventoryRepository = inventoryRepository;
            _logger = logger;

        }

        [HttpGet("")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var inventories = _inventoryRepository.GetAll();
            return View(inventories);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid inventory ID: {InventoryId}", id);
                return BadRequest();
            }

            var inventory = _inventoryRepository.GetById(id);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found for ID: {InventoryId}", id);
                return NotFound();
            }

            return View(inventory);
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
        public IActionResult Create(Inventory inventory)
        {
            ValidateInventoryReferences(inventory);

            if (ModelState.IsValid)
            {
                _inventoryRepository.Add(inventory);
                TempData["ToastTitle"] = "Inventory created";
                TempData["ToastMessage"] = "Inventory item was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        [HttpGet("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Edit(int id)
        {
            var inventory = _inventoryRepository.GetById(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return View(inventory);
        }

        [HttpPost("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Inventory inventory)
        {
            ValidateInventoryReferences(inventory);

            if (id != inventory.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _inventoryRepository.Update(inventory);
                TempData["ToastTitle"] = "Inventory updated";
                TempData["ToastMessage"] = "Inventory item was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(inventory);
        }

        [HttpGet("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid inventory ID for delete: {InventoryId}", id);
                return BadRequest();
            }

            var inventory = _inventoryRepository.GetById(id);

            if (inventory == null)
            {
                _logger.LogWarning("Inventory not found for delete with ID: {InventoryId}", id);
                return NotFound();
            }
            return View(inventory);
        }

        [HttpPost("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _inventoryRepository.Delete(id);
            TempData["ToastTitle"] = "Inventory deleted";
            TempData["ToastMessage"] = "Inventory item was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var inventories = _inventoryRepository.Search(term);
            return PartialView("_InventoryListPartial", inventories);
        }

        private void ValidateInventoryReferences(Inventory inventory)
        {
            if (inventory.ProductId > 0 && !_inventoryRepository.ProductExists(inventory.ProductId))
            {
                ModelState.AddModelError(nameof(Inventory.ProductId), "Selected product does not exist.");
            }

            if (inventory.LocationId > 0 && !_inventoryRepository.LocationExists(inventory.LocationId))
            {
                ModelState.AddModelError(nameof(Inventory.LocationId), "Selected location does not exist.");
            }
        }

    }
}
