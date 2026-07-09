using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Model;
using Microsoft.AspNetCore.Authorization;
using WarehouseManagementSystem.Web.Repositories;

namespace WarehouseManagementSystem.Controllers
{
    [Route("categories")]
    public class CategoryController : Controller
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(CategoryRepository categoryRepository, ILogger<CategoryController> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        [HttpGet("")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var categories = _categoryRepository.GetAll();

            ViewBag.TotalCategories = _categoryRepository.GetTotalCount();
            ViewBag.DocumentedCategories = _categoryRepository.GetDocumentedCount();
            ViewBag.AverageDescriptionLength = _categoryRepository.GetAverageDescriptionLength();

            return View(categories);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid category ID: {CategoryId}", id);
                return BadRequest();

            }

            var category = _categoryRepository.GetById(id);

            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryId}", id);
                return NotFound();
            }

            return View(category);
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
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.Add(category);

                _logger.LogInformation(
                    "User {User} created Category {CategoryId} ({CategoryName})",
                    User.Identity?.Name ?? "Anonymous",
                    category.Id,
                    category.Name);

                TempData["ToastTitle"] = "Category created";
                TempData["ToastMessage"] = "Category was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpGet("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        public IActionResult Edit(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null)
            {
                _logger.LogWarning("Category not found for edit with ID: {CategoryId}", id);
                return NotFound();
            }
            return View(category);
        }

        [HttpPost("edit/{id:int}")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                _logger.LogWarning("Category edit rejected because route ID {RouteId} does not match model ID {ModelId}", id, category.Id);
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _categoryRepository.Update(category);

                _logger.LogInformation(
                    "User {User} updated Category {CategoryId} ({CategoryName})",
                    User.Identity?.Name ?? "Anonymous",
                    category.Id,
                    category.Name);

                TempData["ToastTitle"] = "Category updated";
                TempData["ToastMessage"] = "Category was updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpGet("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid Category ID for delete: {CategoryId}", id);
                return BadRequest();
            }

            var category = _categoryRepository.GetById(id);

            if (category == null)
            {
                _logger.LogWarning("Category not found for delete with ID: {CategoryId}", id);
                return NotFound();
            }
            return View(category);
        }

        [HttpPost("{id:int}/delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (_categoryRepository.HasProducts(id))
            {
                _logger.LogWarning(
                    "User {User} tried to delete Category {CategoryId}, but delete was blocked because it has related products",
                    User.Identity?.Name ?? "Anonymous",
                    id);

                TempData["DeleteError"] = "Category cannot be deleted because it has related products.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var category = _categoryRepository.GetById(id);
            _categoryRepository.Delete(id);

            _logger.LogWarning(
                "User {User} deleted Category {CategoryId} ({CategoryName})",
                User.Identity?.Name ?? "Anonymous",
                id,
                category?.Name ?? "Unknown");

            TempData["ToastTitle"] = "Category deleted";
            TempData["ToastMessage"] = "Category was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public IActionResult Search(string? term)
        {
            var categories = _categoryRepository.Search(term);
            return PartialView("_CategoryListPartial", categories);
        }

        [HttpGet("autocomplete")]
        public IActionResult Autocomplete(string? term)
        {
            var categories = _categoryRepository.Search(term)
                .Take(10)
                .Select(c => new
                {
                    id = c.Id,
                    text = c.Name,
                    subtitle = c.Description
                })
                .ToList();

            return Json(categories);
        }

        [HttpGet("with-products")]
        public IActionResult WithProducts()
        {
            var categories = _categoryRepository.GetWithProducts();

            _logger.LogInformation(
                "User {User} viewed categories with products",
                User.Identity?.Name ?? "Anonymous");

            return View(categories);
        }

        [HttpGet("without-products")]
        public IActionResult WithoutProducts()
        {
            var categories = _categoryRepository.GetWithoutProducts();

            _logger.LogInformation(
                "User {User} viewed categories without products",
                User.Identity?.Name ?? "Anonymous");

            return View(categories);
        }

        [HttpGet("description-keyword")]
        public IActionResult ByDescriptionKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest();
            }

            var categories = _categoryRepository.GetByDescriptionKeyword(keyword);

            ViewBag.Keyword = keyword;

            _logger.LogInformation(
                "User {User} viewed categories with description keyword {Keyword}",
                User.Identity?.Name ?? "Anonymous",
                keyword);

            return View(categories);
        }
    }
}
