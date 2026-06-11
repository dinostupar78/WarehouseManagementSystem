using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/products")]
    public class ProductApiController : ControllerBase
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;

        public ProductApiController(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProductDto>> Get([FromQuery] string? query)
        {
            var productsQuery = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();

                productsQuery = productsQuery.Where(p =>
                    p.Name.ToLower().Contains(query) ||
                    (p.Description != null && p.Description.ToLower().Contains(query)) ||
                    p.Price.ToString().Contains(query) ||
                    p.Weight.ToString().Contains(query) ||
                    p.Category.Name.ToLower().Contains(query));
            }

            var products = productsQuery
                .ToList()
                .Select(ApiMapper.ToDto)
                .ToList();

            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public ActionResult<ProductDto> Get(int id)
        {
            var product = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(ApiMapper.ToDto(product));
        }

        [HttpPost]
        public ActionResult<ProductDto> Post([FromBody] ProductCreateDto dto)
        {
            var categoryExists = _dbContext.Categories
                .AsNoTracking()
                .Any(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Selected category does not exist.");
            }

            var product = ApiMapper.ToEntity(dto);

            this._dbContext.Products.Add(product);
            this._dbContext.SaveChanges();

            var createdProduct = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .First(p => p.Id == product.Id);

            return CreatedAtAction(
                nameof(Get),
                new { id = product.Id },
                ApiMapper.ToDto(createdProduct));
        }

        [HttpPut("{id:int}")]
        public ActionResult<ProductDto> Put(int id, [FromBody] ProductUpdateDto dto)
        {
            var product = _dbContext.Products
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var categoryExists = _dbContext.Categories
                .AsNoTracking()
                .Any(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Selected category does not exist.");
            }

            ApiMapper.UpdateEntity(product, dto);

            this._dbContext.SaveChanges();

            var updatedProduct = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .First(p => p.Id == id);

            return Ok(ApiMapper.ToDto(updatedProduct));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var product = _dbContext.Products
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var hasPurchaseOrderItems = _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Any(poi => poi.ProductId == id);

            if (hasPurchaseOrderItems)
            {
                return Conflict("Product cannot be deleted because it has related purchase order items.");
            }

            _dbContext.Products.Remove(product);
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
