using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    public class SupplierController : Controller
    {
        private readonly SupplierMockRepository _supplierRepository;

        public SupplierController(SupplierMockRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public IActionResult Index()
        {
            var suppliers = _supplierRepository.GetAll();
            return View(suppliers);
        }

        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("Error", "Home");
            }

            var supplier = _supplierRepository.GetById(id);
            if (supplier == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(supplier);
        }

        public IActionResult Error()
        {

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
