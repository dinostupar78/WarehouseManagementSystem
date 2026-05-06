using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Web.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    [Route("suppliers")]
    public class SupplierController : Controller
    {
        private readonly SupplierRepository _supplierRepository;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(SupplierRepository supplierRepository, ILogger<SupplierController> logger)
        {
            _supplierRepository = supplierRepository;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var suppliers = _supplierRepository.GetAll();
            return View(suppliers);
        }

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid supplier ID: {SupplierId}", id);
                return BadRequest();
            }

            var supplier = _supplierRepository.GetById(id);

            if (supplier == null)
            {
                _logger.LogWarning("Supplier not found with ID: {SupplierId}", id);
                return NotFound();
            }

            return View(supplier);
        }
    }
}
