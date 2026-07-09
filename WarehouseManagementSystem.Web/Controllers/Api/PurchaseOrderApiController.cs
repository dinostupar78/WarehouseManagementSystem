using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/purchase-orders")]
    public class PurchaseOrderApiController : ControllerBase
    {
        private readonly WarehouseManagementSystemDbContext _dbContext;
        private readonly ILogger<PurchaseOrderApiController> _logger;

        public PurchaseOrderApiController(WarehouseManagementSystemDbContext dbContext, ILogger<PurchaseOrderApiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PurchaseOrderDto>> Get([FromQuery] string? query)
        {
            var purchaseOrdersQuery = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();

                purchaseOrdersQuery = purchaseOrdersQuery.Where(po =>
                    po.OrderNumber.ToString().Contains(query) ||
                    po.Status.ToString().ToLower().Contains(query) ||
                    po.Supplier.Name.ToLower().Contains(query) ||
                    po.Warehouse.Name.ToLower().Contains(query));
            }

            var purchaseOrders = purchaseOrdersQuery
                .ToList()
                .Select(ApiMapper.ToDto)
                .ToList();

            return Ok(purchaseOrders);
        }

        [HttpGet("{id:int}")]
        public ActionResult<PurchaseOrderDto> Get(int id)
        {
            var purchaseOrder = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .FirstOrDefault(po => po.Id == id);

            if (purchaseOrder == null)
            {
                _logger.LogWarning("API purchase order lookup failed for ID {PurchaseOrderId}", id);
                return NotFound();
            }

            return Ok(ApiMapper.ToDto(purchaseOrder));
        }

        [HttpPost]
        public ActionResult<PurchaseOrderDto> Post([FromBody] PurchaseOrderCreateDto dto)
        {
            if (dto.ExpectedDeliveryDate < dto.OrderDate)
            {
                _logger.LogWarning("API purchase order create rejected because expected delivery date is before order date");
                return BadRequest("Expected delivery date cannot be before the order date.");
            }

            var supplierExists = _dbContext.Suppliers
                .AsNoTracking()
                .Any(s => s.Id == dto.SupplierId);

            if (!supplierExists)
            {
                _logger.LogWarning("API purchase order create rejected because Supplier {SupplierId} does not exist", dto.SupplierId);
                return BadRequest("Selected supplier does not exist.");
            }

            var warehouseExists = _dbContext.Warehouses
                .AsNoTracking()
                .Any(w => w.Id == dto.WarehouseId);

            if (!warehouseExists)
            {
                _logger.LogWarning("API purchase order create rejected because Warehouse {WarehouseId} does not exist", dto.WarehouseId);
                return BadRequest("Selected warehouse does not exist.");
            }

            var nextOrderNumber = _dbContext.PurchaseOrders.Any()
                ? _dbContext.PurchaseOrders.Max(po => po.OrderNumber) + 1
                : 1;

            var purchaseOrder = ApiMapper.ToEntity(dto, nextOrderNumber);

            _dbContext.PurchaseOrders.Add(purchaseOrder);
            _dbContext.SaveChanges();

            var createdPurchaseOrder = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .First(po => po.Id == purchaseOrder.Id);

            _logger.LogInformation("API created PurchaseOrder {PurchaseOrderId} (PO-{OrderNumber})", purchaseOrder.Id, purchaseOrder.OrderNumber);

            return CreatedAtAction(
                nameof(Get),
                new { id = purchaseOrder.Id },
                ApiMapper.ToDto(createdPurchaseOrder));
        }

        [HttpPut("{id:int}")]
        public ActionResult<PurchaseOrderDto> Put(int id, [FromBody] PurchaseOrderUpdateDto dto)
        {
            if (dto.ExpectedDeliveryDate < dto.OrderDate)
            {
                _logger.LogWarning("API purchase order update rejected for PurchaseOrder {PurchaseOrderId} because expected delivery date is before order date", id);
                return BadRequest("Expected delivery date cannot be before the order date.");
            }

            var purchaseOrder = _dbContext.PurchaseOrders
                .FirstOrDefault(po => po.Id == id);

            if (purchaseOrder == null)
            {
                _logger.LogWarning("API purchase order update failed because ID {PurchaseOrderId} was not found", id);
                return NotFound();
            }

            var supplierExists = _dbContext.Suppliers
                .AsNoTracking()
                .Any(s => s.Id == dto.SupplierId);

            if (!supplierExists)
            {
                _logger.LogWarning("API purchase order update rejected for PurchaseOrder {PurchaseOrderId} because Supplier {SupplierId} does not exist", id, dto.SupplierId);
                return BadRequest("Selected supplier does not exist.");
            }

            var warehouseExists = _dbContext.Warehouses
                .AsNoTracking()
                .Any(w => w.Id == dto.WarehouseId);

            if (!warehouseExists)
            {
                _logger.LogWarning("API purchase order update rejected for PurchaseOrder {PurchaseOrderId} because Warehouse {WarehouseId} does not exist", id, dto.WarehouseId);
                return BadRequest("Selected warehouse does not exist.");
            }

            ApiMapper.UpdateEntity(purchaseOrder, dto);

            _dbContext.SaveChanges();

            var updatedPurchaseOrder = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .First(po => po.Id == id);

            _logger.LogInformation("API updated PurchaseOrder {PurchaseOrderId} (PO-{OrderNumber})", purchaseOrder.Id, purchaseOrder.OrderNumber);

            return Ok(ApiMapper.ToDto(updatedPurchaseOrder));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var purchaseOrder = _dbContext.PurchaseOrders
                .FirstOrDefault(po => po.Id == id);

            if (purchaseOrder == null)
            {
                _logger.LogWarning("API purchase order delete failed because ID {PurchaseOrderId} was not found", id);
                return NotFound();
            }

            var hasItems = _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Any(poi => poi.PurchaseOrderId == id);

            if (hasItems)
            {
                _logger.LogWarning("API blocked delete for PurchaseOrder {PurchaseOrderId} because related purchase order items exist", id);
                return Conflict("Purchase order cannot be deleted because it has related purchase order items.");
            }

            _dbContext.PurchaseOrders.Remove(purchaseOrder);
            _dbContext.SaveChanges();

            _logger.LogInformation("API deleted PurchaseOrder {PurchaseOrderId} (PO-{OrderNumber})", purchaseOrder.Id, purchaseOrder.OrderNumber);

            return NoContent();
        }

    }
}
