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

            ViewBag.TotalOrders = _purchaseOrderRepository.GetTotalCount();
            ViewBag.ActiveOrders = _purchaseOrderRepository.GetActiveOrderCount();
            ViewBag.TotalOrderValue = _purchaseOrderRepository.GetTotalOrderValue();

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

                _logger.LogInformation(
                    "User {User} created PurchaseOrder {PurchaseOrderId} (PO-{OrderNumber})",
                    User.Identity?.Name ?? "Anonymous",
                    purchaseOrder.Id,
                    purchaseOrder.OrderNumber);

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
                _logger.LogWarning("Purchase order not found for edit with ID: {PurchaseOrderId}", id);
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
                _logger.LogWarning("Purchase order edit rejected because route ID {RouteId} does not match model ID {ModelId}", id, purchaseOrder.Id);
                return BadRequest();
            }

            var existingPurchaseOrder = _purchaseOrderRepository.GetById(id);
            if (existingPurchaseOrder == null)
            {
                _logger.LogWarning("Purchase order not found while submitting edit for ID: {PurchaseOrderId}", id);
                return NotFound();
            }

            purchaseOrder.OrderNumber = existingPurchaseOrder.OrderNumber;
            ModelState.Remove(nameof(PurchaseOrder.OrderNumber));

            ValidatePurchaseOrderReferences(purchaseOrder);

            if (ModelState.IsValid)
            {
                _purchaseOrderRepository.Update(purchaseOrder);

                _logger.LogInformation(
                    "User {User} updated PurchaseOrder {PurchaseOrderId} (PO-{OrderNumber})",
                    User.Identity?.Name ?? "Anonymous",
                    purchaseOrder.Id,
                    purchaseOrder.OrderNumber);

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
            var purchaseOrder = _purchaseOrderRepository.GetById(id);
            _purchaseOrderRepository.Delete(id);

            _logger.LogWarning(
                "User {User} deleted PurchaseOrder {PurchaseOrderId} (PO-{OrderNumber})",
                User.Identity?.Name ?? "Anonymous",
                id,
                purchaseOrder?.OrderNumber);

            TempData["ToastTitle"] = "Purchase order deleted";
            TempData["ToastMessage"] = "Purchase order was deleted successfully.";
            return RedirectToAction(nameof(Index));
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

        [HttpGet("status/{status}")]
        public IActionResult FindByOrderStatus(string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                _logger.LogWarning(
                    "User {User} tried to filter purchase orders by invalid status {Status}",
                    User.Identity?.Name ?? "Anonymous",
                    status);

                return BadRequest();
            }

            var orders = _purchaseOrderRepository.GetByStatus(parsedStatus);

            ViewBag.Status = parsedStatus;

            _logger.LogInformation(
                "User {User} viewed purchase orders with status {Status}",
                User.Identity?.Name ?? "Anonymous",
                parsedStatus);

            return View(orders);
        }

        [HttpGet("by-supplier/{supplierId:int}")]
        public IActionResult BySupplier(int supplierId)
        {
            var orders = _purchaseOrderRepository.GetBySupplier(supplierId);

            ViewBag.SupplierId = supplierId;
            ViewBag.SupplierName = orders.FirstOrDefault()?.Supplier?.Name ?? $"Supplier {supplierId}";

            _logger.LogInformation(
                "User {User} viewed purchase orders for Supplier {SupplierId}",
                User.Identity?.Name ?? "Anonymous",
                supplierId);

            return View(orders);
        }

        [HttpGet("overdue")]
        public IActionResult Overdue()
        {
            var orders = _purchaseOrderRepository.GetOverdue();

            _logger.LogInformation(
                "User {User} viewed overdue purchase orders",
                User.Identity?.Name ?? "Anonymous");

            return View(orders);
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
