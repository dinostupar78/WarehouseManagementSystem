using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Model;
using Microsoft.AspNetCore.Authorization;
using WarehouseManagementSystem.Web.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    [Route("purchase-order-items")]
    public class PurchaseOrderItemController : Controller
    {
        private readonly PurchaseOrderItemRepository _purchaseOrderItemRepository;
        private readonly ILogger<PurchaseOrderItemController> _logger;

        public PurchaseOrderItemController(PurchaseOrderItemRepository purchaseOrderItemRepository, ILogger<PurchaseOrderItemController> logger)
        {
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _logger = logger;
        }

        [HttpGet("")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var purchaseOrderItems = _purchaseOrderItemRepository.GetAll();
            return View(purchaseOrderItems);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid purchase order item ID: {PurchaseOrderItemId}", id);
                return BadRequest();
            }

            var purchaseOrderItem = _purchaseOrderItemRepository.GetById(id);

            if (purchaseOrderItem == null)
            {
                _logger.LogWarning("Purchase order item not found with ID: {PurchaseOrderItemId}", id);
                return NotFound();
            }

            return View(purchaseOrderItem);
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
        public IActionResult Create(PurchaseOrderItem purchaseOrderItem)
        {
            ValidatePurchaseOrderItemReferences(purchaseOrderItem);

            if (ModelState.IsValid)
            {
                _purchaseOrderItemRepository.Add(purchaseOrderItem);
                TempData["ToastTitle"] = "Purchase order item created";
                TempData["ToastMessage"] = "Purchase order item was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(purchaseOrderItem);
        }

        [HttpGet("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Edit(int id)
        {
            var purchaseOrderItem = _purchaseOrderItemRepository.GetById(id);
            if (purchaseOrderItem == null)
            {
                return NotFound();
            }
            return View(purchaseOrderItem);
        }

        [HttpPost("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, PurchaseOrderItem purchaseOrderItem)
        {
            ValidatePurchaseOrderItemReferences(purchaseOrderItem);

            if (id != purchaseOrderItem.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _purchaseOrderItemRepository.Update(purchaseOrderItem);
                TempData["ToastTitle"] = "Purchase order item updated";
                TempData["ToastMessage"] = "Purchase order item was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(purchaseOrderItem);
        }

        [HttpGet("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid purchase order item ID for delete: {PurchaseOrderItemId}", id);
                return BadRequest();
            }

            var purchaseOrderItem = _purchaseOrderItemRepository.GetById(id);

            if (purchaseOrderItem == null)
            {
                _logger.LogWarning("Purchase order item not found for delete with ID: {PurchaseOrderItemId}", id);
                return NotFound();
            }
            return View(purchaseOrderItem);
        }

        [HttpPost("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _purchaseOrderItemRepository.Delete(id);
            TempData["ToastTitle"] = "Purchase order item deleted";
            TempData["ToastMessage"] = "Purchase order item was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var purchaseOrderItems = _purchaseOrderItemRepository.Search(term);
            return PartialView("_PurchaseOrderItemListPartial", purchaseOrderItems);
        }

        private void ValidatePurchaseOrderItemReferences(PurchaseOrderItem purchaseOrderItem)
        {
            if (purchaseOrderItem.PurchaseOrderId > 0 &&
                !_purchaseOrderItemRepository.PurchaseOrderExists(purchaseOrderItem.PurchaseOrderId))
            {
                ModelState.AddModelError(nameof(PurchaseOrderItem.PurchaseOrderId), "Selected purchase order does not exist.");
            }

            if (purchaseOrderItem.ProductId > 0 && !_purchaseOrderItemRepository.ProductExists(purchaseOrderItem.ProductId))
            {
                ModelState.AddModelError(nameof(PurchaseOrderItem.ProductId), "Selected product does not exist.");
            }
        }
    }
}
