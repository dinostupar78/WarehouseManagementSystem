using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Model;
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
        public IActionResult Index()
        {
            var purchaseOrders = _purchaseOrderRepository.GetAll();
            return View(purchaseOrders);
        }

        [HttpGet("{id:int}")]   
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

            return View("Index", purchaseOrders);
        }

    }
}
