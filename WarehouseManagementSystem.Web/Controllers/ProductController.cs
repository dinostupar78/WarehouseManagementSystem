using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        public IActionResult Index()
        {
            var products = _productRepository.GetAll();

            ViewBag.TotalProducts = _productRepository.GetTotalCount();
            ViewBag.TotalCatalogValue = _productRepository.GetTotalCatalogValue();

            var highestValueProduct = _productRepository.GetHighestValueProduct();
            ViewBag.HighestValueProductName = highestValueProduct?.Name ?? "No products";
            ViewBag.HighestValueProductPrice = highestValueProduct?.Price ?? 0m;

            return View(products);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
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
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            ValidateProductReferences(product);

            if (ModelState.IsValid)
            {
                _productRepository.Add(product);

                _logger.LogInformation(
                    "User {User} created Product {ProductId} ({ProductName})",
                    User.Identity?.Name ?? "Anonymous",
                    product.Id,
                    product.Name);

                TempData["ToastTitle"] = "Product created";
                TempData["ToastMessage"] = "Product was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        [HttpGet("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Edit(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                _logger.LogWarning("Product not found for edit with ID: {ProductId}", id);
                return NotFound();
            }
            return View(product);
        }

        [HttpPost("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Product product)
        {
            ValidateProductReferences(product);

            if (id != product.Id)
            {
                _logger.LogWarning("Product edit rejected because route ID {RouteId} does not match model ID {ModelId}", id, product.Id);
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _productRepository.Update(product);

                _logger.LogInformation(
                    "User {User} updated Product {ProductId} ({ProductName})",
                    User.Identity?.Name ?? "Anonymous",
                    product.Id,
                    product.Name);

                TempData["ToastTitle"] = "Product updated";
                TempData["ToastMessage"] = "Product was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        [HttpGet("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (_productRepository.HasPurchaseOrderItems(id))
            {
                _logger.LogWarning(
                    "User {User} tried to delete Product {ProductId}, but delete was blocked because it has related purchase order items",
                    User.Identity?.Name ?? "Anonymous",
                    id);

                TempData["DeleteError"] = "Product cannot be deleted because it has related purchase order items.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var product = _productRepository.GetById(id);
            _productRepository.Delete(id);

            _logger.LogWarning(
                "User {User} deleted Product {ProductId} ({ProductName})",
                User.Identity?.Name ?? "Anonymous",
                id,
                product?.Name ?? "Unknown");

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
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var products = _productRepository.Search(term);
            return PartialView("_ProductListPartial", products);
        }

        [HttpGet("price-above")]
        public IActionResult PriceAbove(decimal minPrice)
        {
            var products = _productRepository.GetPriceAbove(minPrice);

            ViewBag.MinPrice = minPrice;

            _logger.LogInformation(
                "User {User} viewed products with price above {MinPrice}",
                User.Identity?.Name ?? "Anonymous",
                minPrice);

            return View(products);
        }

        [HttpGet("category/{categoryId:int}")]
        public IActionResult FindByCategory(int categoryId, int? sourceProductId)
        {
            var products = _productRepository.GetByCategory(categoryId);

            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = products.FirstOrDefault()?.Category?.Name ?? $"Category {categoryId}";
            ViewBag.SourceProductId = sourceProductId;

            _logger.LogInformation(
                "User {User} viewed products in Category {CategoryId}",
                User.Identity?.Name ?? "Anonymous",
                categoryId);

            return View("FindByCategory", products);
        }

        [HttpGet("weight-above")]
        public IActionResult WeightAbove(decimal minWeight)
        {
            var products = _productRepository.GetWeightAbove(minWeight);

            ViewBag.MinWeight = minWeight;

            _logger.LogInformation(
                "User {User} viewed products with weight above {MinWeight}",
                User.Identity?.Name ?? "Anonymous",
                minWeight);

            return View(products);
        }

        [HttpGet("similar-weight/{id:int}")]
        public IActionResult FindBySimilarWeight(int id)
        {
            var product = _productRepository.GetById(id);

            if (product == null)
            {
                _logger.LogWarning("Product not found for similar-weight filter: {ProductId}", id);
                return NotFound();
            }

            var minWeight = product.Weight * 0.8m;
            var maxWeight = product.Weight * 1.2m;
            var products = _productRepository.GetByWeightRange(minWeight, maxWeight, product.Id);

            ViewBag.MinWeight = minWeight;
            ViewBag.MaxWeight = maxWeight;
            ViewBag.ProductName = product.Name;
            ViewBag.SourceProductId = product.Id;

            _logger.LogInformation(
                "User {User} viewed products with similar weight to Product {ProductId}",
                User.Identity?.Name ?? "Anonymous",
                product.Id);

            return View("FindBySimilarWeight", products);
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
