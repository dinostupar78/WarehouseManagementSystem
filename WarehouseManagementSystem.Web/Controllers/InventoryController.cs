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

            ViewBag.TotalInventoryItems = _inventoryRepository.GetTotalCount();
            ViewBag.TotalStockUnits = _inventoryRepository.GetTotalStockUnits();
            ViewBag.LowStockItems = _inventoryRepository.GetLowStockCount();

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

                _logger.LogInformation(
                    "User {User} created Inventory {InventoryId} for Product {ProductId} at Location {LocationId}",
                    User.Identity?.Name ?? "Anonymous",
                    inventory.Id,
                    inventory.ProductId,
                    inventory.LocationId);

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
                _logger.LogWarning("Inventory not found for edit with ID: {InventoryId}", id);
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
                _logger.LogWarning("Inventory edit rejected because route ID {RouteId} does not match model ID {ModelId}", id, inventory.Id);
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _inventoryRepository.Update(inventory);

                _logger.LogInformation(
                    "User {User} updated Inventory {InventoryId} for Product {ProductId} at Location {LocationId}",
                    User.Identity?.Name ?? "Anonymous",
                    inventory.Id,
                    inventory.ProductId,
                    inventory.LocationId);

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
            var inventory = _inventoryRepository.GetById(id);
            _inventoryRepository.Delete(id);

            _logger.LogWarning(
                "User {User} deleted Inventory {InventoryId} for Product {ProductId} at Location {LocationId}",
                User.Identity?.Name ?? "Anonymous",
                id,
                inventory?.ProductId,
                inventory?.LocationId);

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

        [HttpGet("low-stock")]
        public IActionResult LowStock(int threshold = 10)
        {
            var inventories = _inventoryRepository.GetLowStock(threshold);

            ViewBag.Threshold = threshold;

            _logger.LogInformation(
                "User {User} viewed inventory items with quantity at or below {Threshold}",
                User.Identity?.Name ?? "Anonymous",
                threshold);

            return View(inventories);
        }

        [HttpGet("by-location/{locationId:int}")]
        public IActionResult ByLocation(int locationId)
        {
            var inventories = _inventoryRepository.GetByLocation(locationId);

            if (!inventories.Any())
            {
                _logger.LogWarning(
                    "User {User} tried to view inventory for Location {LocationId}, but no records were found",
                    User.Identity?.Name ?? "Anonymous",
                    locationId);
            }

            ViewBag.LocationId = locationId;
            ViewBag.LocationCode = inventories.FirstOrDefault()?.Location?.Code ?? $"LOC-{locationId:0000}";

            return View(inventories);
        }

        [HttpGet("by-product/{productId:int}")]
        public IActionResult ByProduct(int productId)
        {
            var inventories = _inventoryRepository.GetByProduct(productId);

            if (!inventories.Any())
            {
                _logger.LogWarning(
                    "User {User} tried to view inventory for Product {ProductId}, but no records were found",
                    User.Identity?.Name ?? "Anonymous",
                    productId);
            }

            ViewBag.ProductId = productId;
            ViewBag.ProductName = inventories.FirstOrDefault()?.Product?.Name ?? $"Product {productId}";

            return View(inventories);
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
