using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly PurchaseOrderMockRepository _purchaseOrderRepository;

        public PurchaseOrderController(PurchaseOrderMockRepository purchaseOrderRepository)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
        }

        public IActionResult Index()
        {
            var purchaseOrders = _purchaseOrderRepository.GetAll();
            return View(purchaseOrders);
        }

        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("Error", "Home");
            }

            var purchaseOrder = _purchaseOrderRepository.GetById(id);
            if (purchaseOrder == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(purchaseOrder);

        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
