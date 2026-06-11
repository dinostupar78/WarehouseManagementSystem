using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Model;
using Microsoft.AspNetCore.Authorization;
using WarehouseManagementSystem.Web.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    [Route("purchase-orders")]
    public class PurchaseOrderController : Controller
    {
        private readonly PurchaseOrderRepository _purchaseOrderRepository;
        private readonly ILogger<PurchaseOrderController> _logger;

        public PurchaseOrderController(PurchaseOrderRepository purchaseOrderRepository, ILogger<PurchaseOrderController> logger)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _logger = logger;
        }

        [HttpGet("")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var purchaseOrders = _purchaseOrderRepository.GetAll();
            return View(purchaseOrders);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid purchase order ID: {PurchaseOrderId}", id);
                return BadRequest();
            }

            var purchaseOrder = _purchaseOrderRepository.GetById(id);

            if (purchaseOrder == null)
            {
                _logger.LogWarning("Purchase order not found with ID: {PurchaseOrderId}", id);
                return NotFound();
            }

            return View(purchaseOrder);

        }

        [HttpGet("create")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Create()
        {
            ViewBag.NextOrderNumber = _purchaseOrderRepository.GetNextOrderNumber();
            return View();
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PurchaseOrder purchaseOrder)
        {
            purchaseOrder.OrderNumber = _purchaseOrderRepository.GetNextOrderNumber();
            ModelState.Remove(nameof(PurchaseOrder.OrderNumber));

            ValidatePurchaseOrderReferences(purchaseOrder);

            if (ModelState.IsValid)
            {
                _purchaseOrderRepository.Add(purchaseOrder);
                TempData["ToastTitle"] = "Purchase order created";
                TempData["ToastMessage"] = "Purchase order was created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(purchaseOrder);
        }

        [HttpGet("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Edit(int id)
        {
            var pruchaseOrder = _purchaseOrderRepository.GetById(id);
            if (pruchaseOrder == null)
            {
                return NotFound();
            }
            return View(pruchaseOrder);
        }

        [HttpPost("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, PurchaseOrder purchaseOrder)
        {
            if (id != purchaseOrder.Id)
            {
                return BadRequest();
            }

            var existingPurchaseOrder = _purchaseOrderRepository.GetById(id);
            if (existingPurchaseOrder == null)
            {
                return NotFound();
            }

            purchaseOrder.OrderNumber = existingPurchaseOrder.OrderNumber;
            ModelState.Remove(nameof(PurchaseOrder.OrderNumber));

            ValidatePurchaseOrderReferences(purchaseOrder);

            if (ModelState.IsValid)
            {
                _purchaseOrderRepository.Update(purchaseOrder);
                TempData["ToastTitle"] = "Purchase order updated";
                TempData["ToastMessage"] = "Purchase order was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(purchaseOrder);
        }

        [HttpGet("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid purchase order ID for delete: {PurchaseOrderId}", id);
                return BadRequest();
            }

            var purchaseOrder = _purchaseOrderRepository.GetById(id);

            if (purchaseOrder == null)
            {
                _logger.LogWarning("Purchase order not found for delete with ID: {PurchaseOrderId}", id);
                return NotFound();
            }
            return View(purchaseOrder);
        }

        [HttpPost("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _purchaseOrderRepository.Delete(id);
            TempData["ToastTitle"] = "Purchase order deleted";
            TempData["ToastMessage"] = "Purchase order was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("status/{status}")]
        public IActionResult FindByOrderStatus(string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                return BadRequest("Invalid status.");
            }

            var purchaseOrders = _purchaseOrderRepository.GetAll()
                .Where(po => po.Status == parsedStatus)
                .ToList();

            ViewBag.Status = parsedStatus;

            return View(purchaseOrders);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var purchaseOrders = _purchaseOrderRepository.Search(term);
            return PartialView("_PurchaseOrderListPartial", purchaseOrders);
        }

        [HttpGet("autocomplete")]
        public IActionResult Autocomplete(string? term)
        {
            var purchaseOrders = _purchaseOrderRepository.Search(term)
                .Take(10)
                .Select(po => new 
                { id = po.Id, 
                  text = $"PO-{po.OrderNumber:0000}",
                  subtitle = $"{po.Supplier?.Name ?? "Unknown Supplier"} - {po.Status}",
                  description = $"Order #{po.OrderNumber} - {po.OrderDate:yyyy-MM-dd}"
                })
                .ToList();
            return Json(purchaseOrders);
        }

        private void ValidatePurchaseOrderReferences(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder.SupplierId > 0 && !_purchaseOrderRepository.SupplierExists(purchaseOrder.SupplierId))
            {
                ModelState.AddModelError(nameof(PurchaseOrder.SupplierId), "Selected supplier does not exist.");
            }

            if (purchaseOrder.WarehouseId > 0 && !_purchaseOrderRepository.WarehouseExists(purchaseOrder.WarehouseId))
            {
                ModelState.AddModelError(nameof(PurchaseOrder.WarehouseId), "Selected warehouse does not exist.");
            }
        }

    }
}
