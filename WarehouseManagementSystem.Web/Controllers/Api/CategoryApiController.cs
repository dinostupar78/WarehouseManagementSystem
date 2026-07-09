using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryApiController : ControllerBase
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;
        private readonly ILogger<CategoryApiController> _logger;

        public CategoryApiController(WarehouseManagementSystemDbContext dbContext, ILogger<CategoryApiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CategoryDto>> Get([FromQuery] string? query)
        {
            var categoriesQuery = _dbContext.Categories
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();

                categoriesQuery = categoriesQuery.Where(c =>
                    c.Name.ToLower().Contains(query) ||
                    (c.Description != null && c.Description.ToLower().Contains(query)));
            }

            var categories = categoriesQuery
                .ToList()
                .Select(ApiMapper.ToDto)
                .ToList();

            return Ok(categories);

        }

        [HttpGet("{id:int}")]
        public ActionResult<CategoryDto> Get(int id) { 
            var category = _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                _logger.LogWarning("API category lookup failed for ID {CategoryId}", id);
                return NotFound();
            }

            return Ok(ApiMapper.ToDto(category));
        }

        [HttpPost]
        public ActionResult<CategoryDto> Post([FromBody] CategoryCreateDto dto)
        {
            var category = ApiMapper.ToEntity(dto);

            this._dbContext.Categories.Add(category);
            this._dbContext.SaveChanges();

            _logger.LogInformation("API created Category {CategoryId} ({CategoryName})", category.Id, category.Name);

            return CreatedAtAction(
                nameof(Get),
                new { id = category.Id },
                ApiMapper.ToDto(category));

        }

        [HttpPut("{id:int}")]
        public ActionResult<CategoryDto> Put(int id, [FromBody] CategoryUpdateDto dto)
        {
           
            var category = _dbContext.Categories
               .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                _logger.LogWarning("API category update failed because ID {CategoryId} was not found", id);
                return NotFound();
            }

            ApiMapper.UpdateEntity(category, dto);

            this._dbContext.SaveChanges();

            _logger.LogInformation("API updated Category {CategoryId} ({CategoryName})", category.Id, category.Name);

            return Ok(ApiMapper.ToDto(category));

        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var category = _dbContext.Categories
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                _logger.LogWarning("API category delete failed because ID {CategoryId} was not found", id);
                return NotFound();
            }

            var hasProducts = _dbContext.Products
                .AsNoTracking()
                .Any(p => p.CategoryId == id);

            if (hasProducts)
            {
                _logger.LogWarning("API blocked delete for Category {CategoryId} because related products exist", id);
                return Conflict("Category cannot be deleted because it has related products.");
            }

            _dbContext.Categories.Remove(category);
            _dbContext.SaveChanges();

            _logger.LogInformation("API deleted Category {CategoryId} ({CategoryName})", category.Id, category.Name);

            return NoContent();
        }

    }
}
