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
    public class PurchaseOrderItemApiTests : IClassFixture<WarehouseManagementSystemWebApplicationFactory>
    {
        private readonly WarehouseManagementSystemWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PurchaseOrderItemApiTests(WarehouseManagementSystemWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnPurchaseOrderItems()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            dbContext.PurchaseOrderItems.AddRange(
                new PurchaseOrderItem
                {
                    Quantity = 2,
                    UnitPrice = 25,
                    PurchaseOrderId = seed.PurchaseOrder.Id,
                    ProductId = seed.Product.Id
                },
                new PurchaseOrderItem
                {
                    Quantity = 5,
                    UnitPrice = 10,
                    PurchaseOrderId = seed.PurchaseOrder.Id,
                    ProductId = seed.Product.Id
                }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/purchase-order-items");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var items = await response.Content.ReadFromJsonAsync<List<PurchaseOrderItemDto>>();

            items.Should().NotBeNull();
            items!.Should().HaveCount(2);
            items.Select(i => i.Quantity).Should().Contain(2);
            items.Select(i => i.Quantity).Should().Contain(5);
        }

        [Fact]
        public async Task Get_ShouldReturnFilteredPurchaseOrderItems_WhenQueryIsProvided()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            dbContext.PurchaseOrderItems.AddRange(
                new PurchaseOrderItem
                {
                    Quantity = 2,
                    UnitPrice = 25,
                    PurchaseOrderId = seed.PurchaseOrder.Id,
                    ProductId = seed.Product.Id
                },
                new PurchaseOrderItem
                {
                    Quantity = 5,
                    UnitPrice = 10,
                    PurchaseOrderId = seed.PurchaseOrder.Id,
                    ProductId = seed.Product.Id
                }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/purchase-order-items?query=25");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var items = await response.Content.ReadFromJsonAsync<List<PurchaseOrderItemDto>>();

            items.Should().NotBeNull();
            items!.Should().ContainSingle();
            items[0].UnitPrice.Should().Be(25);
        }

        [Fact]
        public async Task GetById_ShouldReturnPurchaseOrderItem_WhenPurchaseOrderItemExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            var item = new PurchaseOrderItem
            {
                Quantity = 3,
                UnitPrice = 30,
                PurchaseOrderId = seed.PurchaseOrder.Id,
                ProductId = seed.Product.Id
            };

            dbContext.PurchaseOrderItems.Add(item);
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/purchase-order-items/{item.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<PurchaseOrderItemDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(item.Id);
            dto.Quantity.Should().Be(item.Quantity);
            dto.UnitPrice.Should().Be(item.UnitPrice);
            dto.PurchaseOrder.Should().NotBeNull();
            dto.PurchaseOrder!.Id.Should().Be(seed.PurchaseOrder.Id);
            dto.Product.Should().NotBeNull();
            dto.Product!.Id.Should().Be(seed.Product.Id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenPurchaseOrderItemDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.GetAsync("/api/purchase-order-items/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldCreatePurchaseOrderItem_WhenModelIsValid()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            var createDto = new PurchaseOrderItemCreateDto
            {
                Quantity = 4,
                UnitPrice = 40,
                PurchaseOrderId = seed.PurchaseOrder.Id,
                ProductId = seed.Product.Id
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-order-items", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dto = await response.Content.ReadFromJsonAsync<PurchaseOrderItemDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().BeGreaterThan(0);
            dto.Quantity.Should().Be(createDto.Quantity);
            dto.UnitPrice.Should().Be(createDto.UnitPrice);
            dto.PurchaseOrder.Should().NotBeNull();
            dto.PurchaseOrder!.Id.Should().Be(seed.PurchaseOrder.Id);
            dto.Product.Should().NotBeNull();
            dto.Product!.Id.Should().Be(seed.Product.Id);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            assertDbContext.PurchaseOrderItems.Should().Contain(i => i.Quantity == 4 && i.UnitPrice == 40);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            await ClearDatabaseAsync();

            var invalidDto = new PurchaseOrderItemCreateDto
            {
                Quantity = 0,
                UnitPrice = 0,
                PurchaseOrderId = 0,
                ProductId = 0
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-order-items", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenPurchaseOrderDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            var createDto = new PurchaseOrderItemCreateDto
            {
                Quantity = 4,
                UnitPrice = 40,
                PurchaseOrderId = 999,
                ProductId = seed.Product.Id
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-order-items", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenProductDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            var createDto = new PurchaseOrderItemCreateDto
            {
                Quantity = 4,
                UnitPrice = 40,
                PurchaseOrderId = seed.PurchaseOrder.Id,
                ProductId = 999
            };

            var response = await _client.PostAsJsonAsync("/api/purchase-order-items", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldUpdatePurchaseOrderItem_WhenPurchaseOrderItemExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            var item = new PurchaseOrderItem
            {
                Quantity = 2,
                UnitPrice = 25,
                PurchaseOrderId = seed.PurchaseOrder.Id,
                ProductId = seed.Product.Id
            };

            dbContext.PurchaseOrderItems.Add(item);
            await dbContext.SaveChangesAsync();

            var updateDto = new PurchaseOrderItemUpdateDto
            {
                Quantity = 8,
                UnitPrice = 45,
                PurchaseOrderId = seed.PurchaseOrder.Id,
                ProductId = seed.Product.Id
            };

            var response = await _client.PutAsJsonAsync($"/api/purchase-order-items/{item.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<PurchaseOrderItemDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(item.Id);
            dto.Quantity.Should().Be(updateDto.Quantity);
            dto.UnitPrice.Should().Be(updateDto.UnitPrice);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var updatedItem = await assertDbContext.PurchaseOrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            updatedItem!.Quantity.Should().Be(8);
            updatedItem.UnitPrice.Should().Be(45);
        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenPurchaseOrderItemDoesNotExist()
        {
            await ClearDatabaseAsync();

            var updateDto = new PurchaseOrderItemUpdateDto
            {
                Quantity = 8,
                UnitPrice = 45,
                PurchaseOrderId = 1,
                ProductId = 1
            };

            var response = await _client.PutAsJsonAsync("/api/purchase-order-items/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenPurchaseOrderDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            var item = new PurchaseOrderItem
            {
                Quantity = 2,
                UnitPrice = 25,
                PurchaseOrderId = seed.PurchaseOrder.Id,
                ProductId = seed.Product.Id
            };

            dbContext.PurchaseOrderItems.Add(item);
            await dbContext.SaveChangesAsync();

            var updateDto = new PurchaseOrderItemUpdateDto
            {
                Quantity = 8,
                UnitPrice = 45,
                PurchaseOrderId = 999,
                ProductId = seed.Product.Id
            };

            var response = await _client.PutAsJsonAsync($"/api/purchase-order-items/{item.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenProductDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            var item = new PurchaseOrderItem
            {
                Quantity = 2,
                UnitPrice = 25,
                PurchaseOrderId = seed.PurchaseOrder.Id,
                ProductId = seed.Product.Id
            };

            dbContext.PurchaseOrderItems.Add(item);
            await dbContext.SaveChangesAsync();

            var updateDto = new PurchaseOrderItemUpdateDto
            {
                Quantity = 8,
                UnitPrice = 45,
                PurchaseOrderId = seed.PurchaseOrder.Id,
                ProductId = 999
            };

            var response = await _client.PutAsJsonAsync($"/api/purchase-order-items/{item.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_ShouldRemovePurchaseOrderItem_WhenPurchaseOrderItemExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreatePurchaseOrderItemDependenciesAsync(dbContext);

            var item = new PurchaseOrderItem
            {
                Quantity = 2,
                UnitPrice = 25,
                PurchaseOrderId = seed.PurchaseOrder.Id,
                ProductId = seed.Product.Id
            };

            dbContext.PurchaseOrderItems.Add(item);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/purchase-order-items/{item.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var deletedItem = await assertDbContext.PurchaseOrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            deletedItem.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenPurchaseOrderItemDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.DeleteAsync("/api/purchase-order-items/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private async Task<PurchaseOrderItemSeed> CreatePurchaseOrderItemDependenciesAsync(WarehouseManagementSystemDbContext dbContext)
        {
            var category = new Category
            {
                Name = "Test Category",
                Description = "Category for purchase order item tests"
            };

            var product = new Product
            {
                Name = "Test Product",
                Description = "Product for purchase order item tests",
                Price = 25,
                Weight = 2,
                ProductReceivedAt = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc)
            };

            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Contact",
                ContactEmail = "supplier@purchaseorderitem.test",
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

            dbContext.Categories.Add(category);
            dbContext.Suppliers.Add(supplier);
            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync();

            product.CategoryId = category.Id;
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var purchaseOrder = new PurchaseOrder
            {
                OrderNumber = 1,
                OrderDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 1, 17, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 250,
                Status = OrderStatus.Pending,
                SupplierId = supplier.Id,
                WarehouseId = warehouse.Id
            };

            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            return new PurchaseOrderItemSeed(purchaseOrder, product);
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

        private sealed record PurchaseOrderItemSeed(PurchaseOrder PurchaseOrder, Product Product);
    }
}
