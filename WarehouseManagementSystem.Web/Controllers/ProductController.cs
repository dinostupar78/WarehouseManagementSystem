using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Model;
using WarehouseManagementSystem.Web.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    [Route("products")]
    public class ProductController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ProductRepository productRepository, ILogger<ProductController> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid product ID: {ProductId}", id);
                return BadRequest();
            }

            var product = _productRepository.GetById(id);

            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", id);
                return NotFound();
            }

            return View(product);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            ValidateProductReferences(product);

            if (ModelState.IsValid)
            {
                _productRepository.Add(product);
                TempData["ToastTitle"] = "Product created";
                TempData["ToastMessage"] = "Product was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Product product)
        {
            ValidateProductReferences(product);

            if (id != product.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _productRepository.Update(product);
                TempData["ToastTitle"] = "Product updated";
                TempData["ToastMessage"] = "Product was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        [HttpGet("{id:int}/delete")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid product ID for delete: {ProductId}", id);
                return BadRequest();
            }

            var product = _productRepository.GetById(id);

            if (product == null)
            {
                _logger.LogWarning("Product not found for delete with ID: {ProductId}", id);
                return NotFound();
            }
            return View(product);
        }

        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _productRepository.Delete(id);
            TempData["ToastTitle"] = "Product deleted";
            TempData["ToastMessage"] = "Product was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("autocomplete")]
        public IActionResult Autocomplete(string? term)
        { 
            var products = _productRepository.Search(term)
                .Take(10)
                .Select(p => new 
                { 
                    id = p.Id, 
                    text = p.Name,
                    subtitle = p.Description
                })
                .ToList();

            return Json(products);
        }

        [HttpGet("search")]
        public IActionResult Search(string? term)
        {
            var products = _productRepository.Search(term);
            return PartialView("_ProductListPartial", products);
        }

        [HttpGet("price-above/{minPrice:decimal}")]
        public IActionResult PriceAbove(decimal minPrice)
        {
            var products = _productRepository.GetAll()
                .Where(p => p.Price > minPrice)
                .ToList();

            ViewBag.MinPrice = minPrice;
            return View(products);
        }

        private void ValidateProductReferences(Product product)
        {
            if (product.CategoryId > 0 && !_productRepository.CategoryExists(product.CategoryId))
            {
                ModelState.AddModelError(nameof(Product.CategoryId), "Selected category does not exist.");
            }
        }
    }
}
