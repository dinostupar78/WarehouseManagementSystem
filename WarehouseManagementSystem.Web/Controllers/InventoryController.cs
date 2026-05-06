using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            var inventories = _inventoryRepository.GetAll();
            return View(inventories);
        }

        [HttpGet("{id:int}")]
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

    }
}
