using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/purchase-order-items")]
    public class PurchaseOrderItemApiController : ControllerBase
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;
        private readonly ILogger<PurchaseOrderItemApiController> _logger;

        public PurchaseOrderItemApiController(WarehouseManagementSystemDbContext dbContext, ILogger<PurchaseOrderItemApiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PurchaseOrderItemDto>> Get([FromQuery] string? query)
        {
            var itemsQuery = _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();

                itemsQuery = itemsQuery.Where(poi =>
                    poi.Quantity.ToString().Contains(query) ||
                    poi.UnitPrice.ToString().Contains(query) ||
                    poi.PurchaseOrder.OrderNumber.ToString().Contains(query) ||
                    poi.Product.Name.ToLower().Contains(query));
            }

            var items = itemsQuery
                .ToList()
                .Select(ApiMapper.ToDto)
                .ToList();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public ActionResult<PurchaseOrderItemDto> Get(int id)
        {
            var item = _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .FirstOrDefault(poi => poi.Id == id);

            if (item == null)
            {
                _logger.LogWarning("API purchase order item lookup failed for ID {PurchaseOrderItemId}", id);
                return NotFound();
            }

            return Ok(ApiMapper.ToDto(item));
        }

        [HttpPost]
        public ActionResult<PurchaseOrderItemDto> Post([FromBody] PurchaseOrderItemCreateDto dto)
        {
            var purchaseOrderExists = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Any(po => po.Id == dto.PurchaseOrderId);

            if (!purchaseOrderExists)
            {
                _logger.LogWarning("API purchase order item create rejected because PurchaseOrder {PurchaseOrderId} does not exist", dto.PurchaseOrderId);
                return BadRequest("Selected purchase order does not exist.");
            }

            var productExists = _dbContext.Products
                .AsNoTracking()
                .Any(p => p.Id == dto.ProductId);

            if (!productExists)
            {
                _logger.LogWarning("API purchase order item create rejected because Product {ProductId} does not exist", dto.ProductId);
                return BadRequest("Selected product does not exist.");
            }

            var item = ApiMapper.ToEntity(dto);

            _dbContext.PurchaseOrderItems.Add(item);
            _dbContext.SaveChanges();

            var createdItem = _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .First(poi => poi.Id == item.Id);

            _logger.LogInformation("API created PurchaseOrderItem {PurchaseOrderItemId} for PurchaseOrder {PurchaseOrderId} and Product {ProductId}", item.Id, item.PurchaseOrderId, item.ProductId);

            return CreatedAtAction(
                nameof(Get),
                new { id = item.Id },
                ApiMapper.ToDto(createdItem));
        }

        [HttpPut("{id:int}")]
        public ActionResult<PurchaseOrderItemDto> Put(int id, [FromBody] PurchaseOrderItemUpdateDto dto)
        {
            var item = _dbContext.PurchaseOrderItems
                .FirstOrDefault(poi => poi.Id == id);

            if (item == null)
            {
                _logger.LogWarning("API purchase order item update failed because ID {PurchaseOrderItemId} was not found", id);
                return NotFound();
            }

            var purchaseOrderExists = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Any(po => po.Id == dto.PurchaseOrderId);

            if (!purchaseOrderExists)
            {
                _logger.LogWarning("API purchase order item update rejected for item {PurchaseOrderItemId} because PurchaseOrder {PurchaseOrderId} does not exist", id, dto.PurchaseOrderId);
                return BadRequest("Selected purchase order does not exist.");
            }

            var productExists = _dbContext.Products
                .AsNoTracking()
                .Any(p => p.Id == dto.ProductId);

            if (!productExists)
            {
                _logger.LogWarning("API purchase order item update rejected for item {PurchaseOrderItemId} because Product {ProductId} does not exist", id, dto.ProductId);
                return BadRequest("Selected product does not exist.");
            }

            ApiMapper.UpdateEntity(item, dto);

            _dbContext.SaveChanges();

            var updatedItem = _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .First(poi => poi.Id == id);

            _logger.LogInformation("API updated PurchaseOrderItem {PurchaseOrderItemId} for PurchaseOrder {PurchaseOrderId} and Product {ProductId}", item.Id, item.PurchaseOrderId, item.ProductId);

            return Ok(ApiMapper.ToDto(updatedItem));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var item = _dbContext.PurchaseOrderItems
                .FirstOrDefault(poi => poi.Id == id);

            if (item == null)
            {
                _logger.LogWarning("API purchase order item delete failed because ID {PurchaseOrderItemId} was not found", id);
                return NotFound();
            }

            _dbContext.PurchaseOrderItems.Remove(item);
            _dbContext.SaveChanges();

            _logger.LogInformation("API deleted PurchaseOrderItem {PurchaseOrderItemId}", item.Id);

            return NoContent();
        }
    }
}
