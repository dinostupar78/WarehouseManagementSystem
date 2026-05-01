using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryMockRepository _inventoryRepository;

        public InventoryController(InventoryMockRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public IActionResult Index()
        {
            var inventories = _inventoryRepository.GetAll();
            return View(inventories);
        }

        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("Error", "Home");
            }

            var inventory = _inventoryRepository.GetById(id);
            if (inventory == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(inventory);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
