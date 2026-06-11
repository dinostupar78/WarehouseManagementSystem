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

        public PurchaseOrderApiController(WarehouseManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
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
                return NotFound();
            }

            return Ok(ApiMapper.ToDto(purchaseOrder));
        }

        [HttpPost]
        public ActionResult<PurchaseOrderDto> Post([FromBody] PurchaseOrderCreateDto dto)
        {
            if (dto.ExpectedDeliveryDate < dto.OrderDate)
            {
                return BadRequest("Expected delivery date cannot be before the order date.");
            }

            var supplierExists = _dbContext.Suppliers
                .AsNoTracking()
                .Any(s => s.Id == dto.SupplierId);

            if (!supplierExists)
            {
                return BadRequest("Selected supplier does not exist.");
            }

            var warehouseExists = _dbContext.Warehouses
                .AsNoTracking()
                .Any(w => w.Id == dto.WarehouseId);

            if (!warehouseExists)
            {
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
                return BadRequest("Expected delivery date cannot be before the order date.");
            }

            var purchaseOrder = _dbContext.PurchaseOrders
                .FirstOrDefault(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            var supplierExists = _dbContext.Suppliers
                .AsNoTracking()
                .Any(s => s.Id == dto.SupplierId);

            if (!supplierExists)
            {
                return BadRequest("Selected supplier does not exist.");
            }

            var warehouseExists = _dbContext.Warehouses
                .AsNoTracking()
                .Any(w => w.Id == dto.WarehouseId);

            if (!warehouseExists)
            {
                return BadRequest("Selected warehouse does not exist.");
            }

            ApiMapper.UpdateEntity(purchaseOrder, dto);

            _dbContext.SaveChanges();

            var updatedPurchaseOrder = _dbContext.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .First(po => po.Id == id);

            return Ok(ApiMapper.ToDto(updatedPurchaseOrder));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var purchaseOrder = _dbContext.PurchaseOrders
                .FirstOrDefault(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            var hasItems = _dbContext.PurchaseOrderItems
                .AsNoTracking()
                .Any(poi => poi.PurchaseOrderId == id);

            if (hasItems)
            {
                return Conflict("Purchase order cannot be deleted because it has related purchase order items.");
            }

            _dbContext.PurchaseOrders.Remove(purchaseOrder);
            _dbContext.SaveChanges();

            return NoContent();
        }

    }
}
