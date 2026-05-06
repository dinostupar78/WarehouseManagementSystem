using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            var purchaseOrderItems = _purchaseOrderItemRepository.GetAll();
            return View(purchaseOrderItems);
        }

        [HttpGet("{id:int}")]
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
    }
}
