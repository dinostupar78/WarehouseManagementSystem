using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductMockRepository _productRepository;

        public ProductController(ProductMockRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("Error", "Home");
            }

            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(product);

        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
