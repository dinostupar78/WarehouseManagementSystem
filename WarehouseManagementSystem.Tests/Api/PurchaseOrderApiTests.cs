using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;
using WarehouseManagementSystem.Tests.Infrastructure;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Tests.Api
{
    public class PurchaseOrderApiTests : IClassFixture<WarehouseManagementSystemWebApplicationFactory>
    {
        private readonly WarehouseManagementSystemWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PurchaseOrderApiTests(WarehouseManagementSystemWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnPurchaseOrders()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            dbContext.PurchaseOrders.AddRange(
                CreatePurchaseOrder(1, OrderStatus.Pending, seed.Supplier.Id, seed.Warehouse.Id),
                CreatePurchaseOrder(2, OrderStatus.Delivered, seed.Supplier.Id, seed.Warehouse.Id)
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/purchase-orders");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var purchaseOrders = await response.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();

            purchaseOrders.Should().NotBeNull();
            purchaseOrders!.Should().HaveCount(2);
            purchaseOrders.Select(po => po.OrderNumber).Should().Contain(1);
            purchaseOrders.Select(po => po.OrderNumber).Should().Contain(2);
        }

        [Fact]
        public async Task Get_ShouldReturnFilteredPurchaseOrders_WhenQueryIsProvided()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            dbContext.PurchaseOrders.AddRange(
                CreatePurchaseOrder(1, OrderStatus.Pending, seed.Supplier.Id, seed.Warehouse.Id),
                CreatePurchaseOrder(2, OrderStatus.Delivered, seed.Supplier.Id, seed.Warehouse.Id)
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/purchase-orders?query=delivered");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var purchaseOrders = await response.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();

            purchaseOrders.Should().NotBeNull();
            purchaseOrders!.Should().ContainSingle();
            purchaseOrders[0].Status.Should().Be(OrderStatus.Delivered);
        }

        [Fact]
        public async Task GetById_ShouldReturnPurchaseOrder_WhenPurchaseOrderExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            var purchaseOrder = CreatePurchaseOrder(10, OrderStatus.Pending, seed.Supplier.Id, seed.Warehouse.Id);

            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/purchase-orders/{purchaseOrder.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<PurchaseOrderDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(purchaseOrder.Id);
            dto.OrderNumber.Should().Be(purchaseOrder.OrderNumber);
            dto.OrderDate.Should().Be(purchaseOrder.OrderDate);
            dto.ExpectedDeliveryDate.Should().Be(purchaseOrder.ExpectedDeliveryDate);
            dto.TotalAmount.Should().Be(purchaseOrder.TotalAmount);
            dto.Status.Should().Be(purchaseOrder.Status);
            dto.Supplier.Should().NotBeNull();
            dto.Supplier!.Id.Should().Be(seed.Supplier.Id);
            dto.Warehouse.Should().NotBeNull();
            dto.Warehouse!.Id.Should().Be(seed.Warehouse.Id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenPurchaseOrderDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.GetAsync("/api/purchase-orders/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldCreatePurchaseOrder_WhenModelIsValid()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);
            var orderDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc);
            var expectedDeliveryDate = new DateTime(2026, 1, 17, 8, 0, 0, DateTimeKind.Utc);

            var createDto = new PurchaseOrderCreateDto
            {
                OrderDate = orderDate,
                ExpectedDeliveryDate = expectedDeliveryDate,
                TotalAmount = 500,
                Status = OrderStatus.Pending,
                SupplierId = seed.Supplier.Id,
                WarehouseId = seed.Warehouse.Id
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-orders", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dto = await response.Content.ReadFromJsonAsync<PurchaseOrderDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().BeGreaterThan(0);
            dto.OrderNumber.Should().Be(1);
            dto.OrderDate.Should().Be(orderDate);
            dto.ExpectedDeliveryDate.Should().Be(expectedDeliveryDate);
            dto.TotalAmount.Should().Be(createDto.TotalAmount);
            dto.Status.Should().Be(createDto.Status);
            dto.Supplier.Should().NotBeNull();
            dto.Supplier!.Id.Should().Be(seed.Supplier.Id);
            dto.Warehouse.Should().NotBeNull();
            dto.Warehouse!.Id.Should().Be(seed.Warehouse.Id);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            assertDbContext.PurchaseOrders.Should().Contain(po => po.OrderNumber == 1);
        }

        [Fact]
        public async Task Post_ShouldAssignNextOrderNumber_WhenPurchaseOrdersAlreadyExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            dbContext.PurchaseOrders.Add(CreatePurchaseOrder(7, OrderStatus.Pending, seed.Supplier.Id, seed.Warehouse.Id));
            await dbContext.SaveChangesAsync();

            var createDto = new PurchaseOrderCreateDto
            {
                OrderDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 1, 17, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 500,
                Status = OrderStatus.Pending,
                SupplierId = seed.Supplier.Id,
                WarehouseId = seed.Warehouse.Id
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-orders", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dto = await response.Content.ReadFromJsonAsync<PurchaseOrderDto>();

            dto.Should().NotBeNull();
            dto!.OrderNumber.Should().Be(8);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            await ClearDatabaseAsync();

            var invalidDto = new PurchaseOrderCreateDto
            {
                OrderDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 1, 17, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = -1,
                Status = OrderStatus.Pending,
                SupplierId = 0,
                WarehouseId = 0
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-orders", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenExpectedDeliveryDateIsBeforeOrderDate()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            var createDto = new PurchaseOrderCreateDto
            {
                OrderDate = new DateTime(2026, 1, 20, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 100,
                Status = OrderStatus.Pending,
                SupplierId = seed.Supplier.Id,
                WarehouseId = seed.Warehouse.Id
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-orders", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenSupplierDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            var createDto = new PurchaseOrderCreateDto
            {
                OrderDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 1, 17, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 100,
                Status = OrderStatus.Pending,
                SupplierId = 999,
                WarehouseId = seed.Warehouse.Id
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-orders", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenWarehouseDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            var createDto = new PurchaseOrderCreateDto
            {
                OrderDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 1, 17, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 100,
                Status = OrderStatus.Pending,
                SupplierId = seed.Supplier.Id,
                WarehouseId = 999
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-orders", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldUpdatePurchaseOrder_WhenPurchaseOrderExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            var purchaseOrder = CreatePurchaseOrder(3, OrderStatus.Pending, seed.Supplier.Id, seed.Warehouse.Id);

            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            var updateDto = new PurchaseOrderUpdateDto
            {
                OrderDate = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 2, 8, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 900,
                Status = OrderStatus.Delivered,
                SupplierId = seed.Supplier.Id,
                WarehouseId = seed.Warehouse.Id
            };

            var response = await _client.PutAsJsonAsync($"/api/purchase-orders/{purchaseOrder.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<PurchaseOrderDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(purchaseOrder.Id);
            dto.OrderNumber.Should().Be(purchaseOrder.OrderNumber);
            dto.OrderDate.Should().Be(updateDto.OrderDate);
            dto.ExpectedDeliveryDate.Should().Be(updateDto.ExpectedDeliveryDate);
            dto.TotalAmount.Should().Be(updateDto.TotalAmount);
            dto.Status.Should().Be(updateDto.Status);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var updatedPurchaseOrder = await assertDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id);

            updatedPurchaseOrder!.Status.Should().Be(OrderStatus.Delivered);
            updatedPurchaseOrder.TotalAmount.Should().Be(900);
        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenPurchaseOrderDoesNotExist()
        {
            await ClearDatabaseAsync();

            var updateDto = new PurchaseOrderUpdateDto
            {
                OrderDate = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 2, 8, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 900,
                Status = OrderStatus.Delivered,
                SupplierId = 1,
                WarehouseId = 1
            };

            var response = await _client.PutAsJsonAsync("/api/purchase-orders/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenExpectedDeliveryDateIsBeforeOrderDate()
        {
            await ClearDatabaseAsync();

            var updateDto = new PurchaseOrderUpdateDto
            {
                OrderDate = new DateTime(2026, 2, 8, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 900,
                Status = OrderStatus.Delivered,
                SupplierId = 1,
                WarehouseId = 1
            };

            var response = await _client.PutAsJsonAsync("/api/purchase-orders/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenSupplierDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            var purchaseOrder = CreatePurchaseOrder(4, OrderStatus.Pending, seed.Supplier.Id, seed.Warehouse.Id);

            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            var updateDto = new PurchaseOrderUpdateDto
            {
                OrderDate = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 2, 8, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 900,
                Status = OrderStatus.Delivered,
                SupplierId = 999,
                WarehouseId = seed.Warehouse.Id
            };

            var response = await _client.PutAsJsonAsync($"/api/purchase-orders/{purchaseOrder.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenWarehouseDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            var purchaseOrder = CreatePurchaseOrder(5, OrderStatus.Pending, seed.Supplier.Id, seed.Warehouse.Id);

            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            var updateDto = new PurchaseOrderUpdateDto
            {
                OrderDate = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 2, 8, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 900,
                Status = OrderStatus.Delivered,
                SupplierId = seed.Supplier.Id,
                WarehouseId = 999
            };

            var response = await _client.PutAsJsonAsync($"/api/purchase-orders/{purchaseOrder.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_ShouldRemovePurchaseOrder_WhenPurchaseOrderExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);

            var purchaseOrder = CreatePurchaseOrder(6, OrderStatus.Pending, seed.Supplier.Id, seed.Warehouse.Id);

            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/purchase-orders/{purchaseOrder.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var deletedPurchaseOrder = await assertDbContext.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id);

            deletedPurchaseOrder.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenPurchaseOrderDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.DeleteAsync("/api/purchase-orders/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnConflict_WhenPurchaseOrderHasItems()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderDependenciesAsync(dbContext);
            var category = await CreateCategoryAsync(dbContext);

            var product = new Product
            {
                Name = "Test Product",
                Description = "Product for purchase order item",
                Price = 25,
                Weight = 2,
                ProductReceivedAt = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                CategoryId = category.Id
            };

            var purchaseOrder = CreatePurchaseOrder(9, OrderStatus.Pending, seed.Supplier.Id, seed.Warehouse.Id);

            dbContext.Products.Add(product);
            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            dbContext.PurchaseOrderItems.Add(new PurchaseOrderItem
            {
                Quantity = 2,
                UnitPrice = 25,
                ProductId = product.Id,
                PurchaseOrderId = purchaseOrder.Id
            });

            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/purchase-orders/{purchaseOrder.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            var existingPurchaseOrder = await dbContext.PurchaseOrders.FindAsync(purchaseOrder.Id);
            existingPurchaseOrder.Should().NotBeNull();
        }

        private async Task<PurchaseOrderSeed> CreatePurchaseOrderDependenciesAsync(WarehouseManagementSystemDbContext dbContext)
        {
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Contact",
                ContactEmail = "supplier@purchaseorder.test",
                ContactPhone = "+385911234567",
                ContactAddress = "Supplier Address"
            };

            var warehouse = new Warehouse
            {
                Name = "Test Warehouse",
                Address = "100 Test Street",
                City = "Zagreb",
                Country = "Croatia",
                Capacity = 5000
            };

            dbContext.Suppliers.Add(supplier);
            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync();

            return new PurchaseOrderSeed(supplier, warehouse);
        }

        private async Task<Category> CreateCategoryAsync(WarehouseManagementSystemDbContext dbContext)
        {
            var category = new Category
            {
                Name = "Test Category",
                Description = "Category for purchase order tests"
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            return category;
        }

        private static PurchaseOrder CreatePurchaseOrder(
            int orderNumber,
            OrderStatus status,
            int supplierId,
            int warehouseId)
        {
            return new PurchaseOrder
            {
                OrderNumber = orderNumber,
                OrderDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 1, 17, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 250,
                Status = status,
                SupplierId = supplierId,
                WarehouseId = warehouseId
            };
        }

        private async Task ClearDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.PurchaseOrderItems.RemoveRange(dbContext.PurchaseOrderItems);
            dbContext.PurchaseOrders.RemoveRange(dbContext.PurchaseOrders);
            dbContext.Products.RemoveRange(dbContext.Products);
            dbContext.Categories.RemoveRange(dbContext.Categories);
            dbContext.Suppliers.RemoveRange(dbContext.Suppliers);
            dbContext.Warehouses.RemoveRange(dbContext.Warehouses);

            await dbContext.SaveChangesAsync();
        }

        private sealed record PurchaseOrderSeed(Supplier Supplier, Warehouse Warehouse);
    }
}
