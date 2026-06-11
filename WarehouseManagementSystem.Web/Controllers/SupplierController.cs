using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Model;
using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        public IActionResult Index()
        {
            var suppliers = _supplierRepository.GetAll();
            return View(suppliers);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
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

        [HttpGet("create")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                _supplierRepository.Add(supplier);
                TempData["ToastTitle"] = "Supplier created";
                TempData["ToastMessage"] = "Supplier was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        [HttpGet("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Edit(int id)
        {
            var supplier = _supplierRepository.GetById(id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        [HttpPost("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _supplierRepository.Update(supplier);
                TempData["ToastTitle"] = "Supplier updated";
                TempData["ToastMessage"] = "Supplier was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }

        [HttpGet("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid supplier ID for delete: {SupplierId}", id);
                return BadRequest();
            }

            var supplier = _supplierRepository.GetById(id);

            if (supplier == null)
            {
                _logger.LogWarning("Supplier not found for delete with ID: {SupplierId}", id);
                return NotFound();
            }
            return View(supplier);
        }

        [HttpPost("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (_supplierRepository.HasPurchaseOrders(id))
            {
                TempData["DeleteError"] = "Supplier cannot be deleted because it has related purchase orders.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _supplierRepository.Delete(id);
            TempData["ToastTitle"] = "Supplier deleted";
            TempData["ToastMessage"] = "Supplier was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var suppliers = _supplierRepository.Search(term);
            return PartialView("_SupplierListPartial", suppliers);
        }

        [HttpGet("autocomplete")]
        public IActionResult Autocomplete(string? term)
        {
            var suppliers = _supplierRepository.Search(term)
                .Take(10)
                .Select(s => new 
                { id = s.Id, 
                  text = s.Name,
                  subtitle = $"{s.ContactPerson}, {s.ContactEmail}"
                })
                .ToList();

            return Json(suppliers);
        }
    }
}
