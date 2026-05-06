using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("price-above/{minPrice:decimal}")]
        public IActionResult PriceAbove(decimal minPrice)
        {
            var products = _productRepository.GetAll()
                .Where(p => p.Price > minPrice)
                .ToList();

            ViewBag.MinPrice = minPrice;
            return View(products);
        }
    }
}
