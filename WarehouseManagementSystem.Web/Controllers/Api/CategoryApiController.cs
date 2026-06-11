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

        public CategoryApiController(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
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
                return NotFound();
            }

            ApiMapper.UpdateEntity(category, dto);

            this._dbContext.SaveChanges();

            return Ok(ApiMapper.ToDto(category));

        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var category = _dbContext.Categories
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var hasProducts = _dbContext.Products
                .AsNoTracking()
                .Any(p => p.CategoryId == id);

            if (hasProducts)
            {
                return Conflict("Category cannot be deleted because it has related products.");
            }

            _dbContext.Categories.Remove(category);
            _dbContext.SaveChanges();

            return NoContent();
        }

    }
}
