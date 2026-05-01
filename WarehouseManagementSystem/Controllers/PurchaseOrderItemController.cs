using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    public class PurchaseOrderItemController : Controller
    {
        private readonly PurchaseOrderItemMockRepository _purchaseOrderItemRepository;

        public PurchaseOrderItemController(PurchaseOrderItemMockRepository purchaseOrderItemRepository)
        {
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
        }

        public IActionResult Index()
        {
            var purchaseOrderItems = _purchaseOrderItemRepository.GetAll();
            return View(purchaseOrderItems);
        }

        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("Error", "Home");
            }

            var purchaseOrderItem = _purchaseOrderItemRepository.GetById(id);
            if (purchaseOrderItem == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(purchaseOrderItem);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
