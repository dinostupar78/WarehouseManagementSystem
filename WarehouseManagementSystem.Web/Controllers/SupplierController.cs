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

            ViewBag.TotalSuppliers = _supplierRepository.GetTotalCount();
            ViewBag.SuppliersWithEmail = _supplierRepository.GetSuppliersWithEmailCount();
            ViewBag.SuppliersWithAddress = _supplierRepository.GetSuppliersWithAddressCount();

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

                _logger.LogInformation(
                    "User {User} created Supplier {SupplierId} ({SupplierName})",
                    User.Identity?.Name ?? "Anonymous",
                    supplier.Id,
                    supplier.Name);

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
                _logger.LogWarning("Supplier not found for edit with ID: {SupplierId}", id);
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
                _logger.LogWarning("Supplier edit rejected because route ID {RouteId} does not match model ID {ModelId}", id, supplier.Id);
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _supplierRepository.Update(supplier);

                _logger.LogInformation(
                    "User {User} updated Supplier {SupplierId} ({SupplierName})",
                    User.Identity?.Name ?? "Anonymous",
                    supplier.Id,
                    supplier.Name);

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
                _logger.LogWarning(
                    "User {User} tried to delete Supplier {SupplierId}, but delete was blocked because it has related purchase orders",
                    User.Identity?.Name ?? "Anonymous",
                    id);

                TempData["DeleteError"] = "Supplier cannot be deleted because it has related purchase orders.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var supplier = _supplierRepository.GetById(id);
            _supplierRepository.Delete(id);

            _logger.LogWarning(
                "User {User} deleted Supplier {SupplierId} ({SupplierName})",
                User.Identity?.Name ?? "Anonymous",
                id,
                supplier?.Name ?? "Unknown");

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

        [HttpGet("by-email-domain")]
        public IActionResult ByEmailDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return BadRequest();
            }

            var suppliers = _supplierRepository.GetByEmailDomain(domain);

            ViewBag.EmailDomain = domain;

            _logger.LogInformation(
                "User {User} viewed suppliers with email domain {Domain}",
                User.Identity?.Name ?? "Anonymous",
                domain);

            return View(suppliers);
        }

        [HttpGet("with-purchase-orders")]
        public IActionResult WithPurchaseOrders()
        {
            var suppliers = _supplierRepository.GetWithPurchaseOrders();

            _logger.LogInformation(
                "User {User} viewed suppliers with purchase orders",
                User.Identity?.Name ?? "Anonymous");

            return View(suppliers);
        }

        [HttpGet("without-purchase-orders")]
        public IActionResult WithoutPurchaseOrders()
        {
            var suppliers = _supplierRepository.GetWithoutPurchaseOrders();

            _logger.LogInformation(
                "User {User} viewed suppliers without purchase orders",
                User.Identity?.Name ?? "Anonymous");

            return View(suppliers);
        }
    }
}
