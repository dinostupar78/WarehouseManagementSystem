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
        private readonly ILogger<ProductApiController> _logger;

        public ProductApiController(WarehouseManagementSystemDbContext dbContext, ILogger<ProductApiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
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
                _logger.LogWarning("API product lookup failed for ID {ProductId}", id);
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
                _logger.LogWarning("API product create rejected because Category {CategoryId} does not exist", dto.CategoryId);
                return BadRequest("Selected category does not exist.");
            }

            var product = ApiMapper.ToEntity(dto);

            this._dbContext.Products.Add(product);
            this._dbContext.SaveChanges();

            var createdProduct = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .First(p => p.Id == product.Id);

            _logger.LogInformation("API created Product {ProductId} ({ProductName})", product.Id, product.Name);

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
                _logger.LogWarning("API product update failed because ID {ProductId} was not found", id);
                return NotFound();
            }

            var categoryExists = _dbContext.Categories
                .AsNoTracking()
                .Any(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                _logger.LogWarning("API product update rejected for Product {ProductId} because Category {CategoryId} does not exist", id, dto.CategoryId);
                return BadRequest("Selected category does not exist.");
            }

            ApiMapper.UpdateEntity(product, dto);

            this._dbContext.SaveChanges();

            var updatedProduct = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .First(p => p.Id == id);

            _logger.LogInformation("API updated Product {ProductId} ({ProductName})", product.Id, product.Name);

            return Ok(ApiMapper.ToDto(updatedProduct));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var product = _dbContext.Products
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning("API product delete failed because ID {ProductId} was not found", id);
                return NotFound();
            }

            var hasPurchaseOrderItems = _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Any(poi => poi.ProductId == id);

            if (hasPurchaseOrderItems)
            {
                _logger.LogWarning("API blocked delete for Product {ProductId} because related purchase order items exist", id);
                return Conflict("Product cannot be deleted because it has related purchase order items.");
            }

            _dbContext.Products.Remove(product);
            _dbContext.SaveChanges();

            _logger.LogInformation("API deleted Product {ProductId} ({ProductName})", product.Id, product.Name);

            return NoContent();
        }
    }
}
