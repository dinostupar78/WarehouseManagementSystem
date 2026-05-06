using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            var categories = _categoryRepository.GetAll();
            return View(categories);
        }

        [HttpGet("{id:int}")]
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
    }
}
