using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Web.Repositories;

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
