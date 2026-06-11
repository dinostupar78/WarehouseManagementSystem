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

        public PurchaseOrderItemApiController(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
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
                return BadRequest("Selected purchase order does not exist.");
            }

            var productExists = _dbContext.Products
                .AsNoTracking()
                .Any(p => p.Id == dto.ProductId);

            if (!productExists)
            {
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
                return NotFound();
            }

            var purchaseOrderExists = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Any(po => po.Id == dto.PurchaseOrderId);

            if (!purchaseOrderExists)
            {
                return BadRequest("Selected purchase order does not exist.");
            }

            var productExists = _dbContext.Products
                .AsNoTracking()
                .Any(p => p.Id == dto.ProductId);

            if (!productExists)
            {
                return BadRequest("Selected product does not exist.");
            }

            ApiMapper.UpdateEntity(item, dto);

            _dbContext.SaveChanges();

            var updatedItem = _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Include(poi => poi.PurchaseOrder)
                .Include(poi => poi.Product)
                .First(poi => poi.Id == id);

            return Ok(ApiMapper.ToDto(updatedItem));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var item = _dbContext.PurchaseOrderItems
                .FirstOrDefault(poi => poi.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            _dbContext.PurchaseOrderItems.Remove(item);
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
