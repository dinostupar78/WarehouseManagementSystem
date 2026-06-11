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
    public class InventoryApiTests : IClassFixture<WarehouseManagementSystemWebApplicationFactory>
    {
        private readonly WarehouseManagementSystemWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public InventoryApiTests(WarehouseManagementSystemWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnInventories()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);

            dbContext.Inventories.AddRange(
                new Inventory
                {
                    Quantity = 25,
                    LastUpdated = new DateTime(2026, 1, 10, 8, 30, 0, DateTimeKind.Utc),
                    ProductId = seed.Product.Id,
                    LocationId = seed.Location.Id
                },
                new Inventory
                {
                    Quantity = 5,
                    LastUpdated = new DateTime(2026, 1, 11, 9, 0, 0, DateTimeKind.Utc),
                    ProductId = seed.Product.Id,
                    LocationId = seed.Location.Id
                }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/inventories");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var inventories = await response.Content.ReadFromJsonAsync<List<InventoryDto>>();

            inventories.Should().NotBeNull();
            inventories!.Should().HaveCount(2);
            inventories.Select(i => i.Quantity).Should().Contain(25);
            inventories.Select(i => i.Quantity).Should().Contain(5);
        }

        [Fact]
        public async Task Get_ShouldReturnFilteredInventories_WhenQueryIsProvided()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);

            dbContext.Inventories.AddRange(
                new Inventory
                {
                    Quantity = 25,
                    LastUpdated = new DateTime(2026, 1, 10, 8, 30, 0, DateTimeKind.Utc),
                    ProductId = seed.Product.Id,
                    LocationId = seed.Location.Id
                },
                new Inventory
                {
                    Quantity = 5,
                    LastUpdated = new DateTime(2026, 1, 11, 9, 0, 0, DateTimeKind.Utc),
                    ProductId = seed.Product.Id,
                    LocationId = seed.Location.Id
                }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/inventories?query=25");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var inventories = await response.Content.ReadFromJsonAsync<List<InventoryDto>>();

            inventories.Should().NotBeNull();
            inventories!.Should().ContainSingle();
            inventories[0].Quantity.Should().Be(25);
        }

        [Fact]
        public async Task GetById_ShouldReturnInventory_WhenInventoryExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);

            var inventory = new Inventory
            {
                Quantity = 30,
                LastUpdated = new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc),
                ProductId = seed.Product.Id,
                LocationId = seed.Location.Id
            };

            dbContext.Inventories.Add(inventory);
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/inventories/{inventory.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<InventoryDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(inventory.Id);
            dto.Quantity.Should().Be(inventory.Quantity);
            dto.LastUpdated.Should().Be(inventory.LastUpdated);
            dto.Product.Should().NotBeNull();
            dto.Product!.Id.Should().Be(seed.Product.Id);
            dto.Location.Should().NotBeNull();
            dto.Location!.Id.Should().Be(seed.Location.Id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenInventoryDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.GetAsync("/api/inventories/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldCreateInventory_WhenModelIsValid()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);
            var lastUpdated = new DateTime(2026, 1, 13, 11, 15, 0, DateTimeKind.Utc);

            var createDto = new InventoryCreateDto
            {
                Quantity = 40,
                LastUpdated = lastUpdated,
                ProductId = seed.Product.Id,
                LocationId = seed.Location.Id
            };

            var response = await _client.PostAsJsonAsync("/api/inventories", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dto = await response.Content.ReadFromJsonAsync<InventoryDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().BeGreaterThan(0);
            dto.Quantity.Should().Be(createDto.Quantity);
            dto.LastUpdated.Should().Be(lastUpdated);
            dto.Product.Should().NotBeNull();
            dto.Product!.Id.Should().Be(seed.Product.Id);
            dto.Location.Should().NotBeNull();
            dto.Location!.Id.Should().Be(seed.Location.Id);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            assertDbContext.Inventories.Should().Contain(i => i.Quantity == 40);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            await ClearDatabaseAsync();

            var invalidDto = new InventoryCreateDto
            {
                Quantity = -1,
                LastUpdated = new DateTime(2026, 1, 13, 11, 15, 0, DateTimeKind.Utc),
                ProductId = 0,
                LocationId = 0
            };

            var response = await _client.PostAsJsonAsync("/api/inventories", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenProductDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);

            var createDto = new InventoryCreateDto
            {
                Quantity = 10,
                LastUpdated = new DateTime(2026, 1, 13, 11, 15, 0, DateTimeKind.Utc),
                ProductId = 999,
                LocationId = seed.Location.Id
            };

            var response = await _client.PostAsJsonAsync("/api/inventories", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenLocationDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);

            var createDto = new InventoryCreateDto
            {
                Quantity = 10,
                LastUpdated = new DateTime(2026, 1, 13, 11, 15, 0, DateTimeKind.Utc),
                ProductId = seed.Product.Id,
                LocationId = 999
            };

            var response = await _client.PostAsJsonAsync("/api/inventories", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldUpdateInventory_WhenInventoryExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);

            var inventory = new Inventory
            {
                Quantity = 15,
                LastUpdated = new DateTime(2026, 1, 10, 8, 30, 0, DateTimeKind.Utc),
                ProductId = seed.Product.Id,
                LocationId = seed.Location.Id
            };

            dbContext.Inventories.Add(inventory);
            await dbContext.SaveChangesAsync();

            var updateDto = new InventoryUpdateDto
            {
                Quantity = 75,
                LastUpdated = new DateTime(2026, 1, 14, 12, 0, 0, DateTimeKind.Utc),
                ProductId = seed.Product.Id,
                LocationId = seed.Location.Id
            };

            var response = await _client.PutAsJsonAsync($"/api/inventories/{inventory.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<InventoryDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(inventory.Id);
            dto.Quantity.Should().Be(updateDto.Quantity);
            dto.LastUpdated.Should().Be(updateDto.LastUpdated);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var updatedInventory = await assertDbContext.Inventories
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == inventory.Id);

            updatedInventory!.Quantity.Should().Be(75);
            updatedInventory.LastUpdated.Should().Be(updateDto.LastUpdated);
        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenInventoryDoesNotExist()
        {
            await ClearDatabaseAsync();

            var updateDto = new InventoryUpdateDto
            {
                Quantity = 75,
                LastUpdated = new DateTime(2026, 1, 14, 12, 0, 0, DateTimeKind.Utc),
                ProductId = 1,
                LocationId = 1
            };

            var response = await _client.PutAsJsonAsync("/api/inventories/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenProductDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);

            var inventory = new Inventory
            {
                Quantity = 15,
                LastUpdated = new DateTime(2026, 1, 10, 8, 30, 0, DateTimeKind.Utc),
                ProductId = seed.Product.Id,
                LocationId = seed.Location.Id
            };

            dbContext.Inventories.Add(inventory);
            await dbContext.SaveChangesAsync();

            var updateDto = new InventoryUpdateDto
            {
                Quantity = 75,
                LastUpdated = new DateTime(2026, 1, 14, 12, 0, 0, DateTimeKind.Utc),
                ProductId = 999,
                LocationId = seed.Location.Id
            };

            var response = await _client.PutAsJsonAsync($"/api/inventories/{inventory.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenLocationDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);

            var inventory = new Inventory
            {
                Quantity = 15,
                LastUpdated = new DateTime(2026, 1, 10, 8, 30, 0, DateTimeKind.Utc),
                ProductId = seed.Product.Id,
                LocationId = seed.Location.Id
            };

            dbContext.Inventories.Add(inventory);
            await dbContext.SaveChangesAsync();

            var updateDto = new InventoryUpdateDto
            {
                Quantity = 75,
                LastUpdated = new DateTime(2026, 1, 14, 12, 0, 0, DateTimeKind.Utc),
                ProductId = seed.Product.Id,
                LocationId = 999
            };

            var response = await _client.PutAsJsonAsync($"/api/inventories/{inventory.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_ShouldRemoveInventory_WhenInventoryExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var seed = await CreateInventoryDependenciesAsync(dbContext);

            var inventory = new Inventory
            {
                Quantity = 15,
                LastUpdated = new DateTime(2026, 1, 10, 8, 30, 0, DateTimeKind.Utc),
                ProductId = seed.Product.Id,
                LocationId = seed.Location.Id
            };

            dbContext.Inventories.Add(inventory);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/inventories/{inventory.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var deletedInventory = await assertDbContext.Inventories
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == inventory.Id);

            deletedInventory.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenInventoryDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.DeleteAsync("/api/inventories/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private async Task<InventorySeed> CreateInventoryDependenciesAsync(WarehouseManagementSystemDbContext dbContext)
        {
            var category = new Category
            {
                Name = "Test Category",
                Description = "Category for inventory tests"
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
            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                Description = "Product for inventory tests",
                Price = 20,
                Weight = 3,
                ProductReceivedAt = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                CategoryId = category.Id
            };

            var location = new Location
            {
                Code = "INV-A-01",
                Zone = "A",
                ShelfNumber = 1,
                WarehouseId = warehouse.Id
            };

            dbContext.Products.Add(product);
            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return new InventorySeed(product, location);
        }

        private async Task ClearDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Inventories.RemoveRange(dbContext.Inventories);
            dbContext.Products.RemoveRange(dbContext.Products);
            dbContext.Locations.RemoveRange(dbContext.Locations);
            dbContext.Categories.RemoveRange(dbContext.Categories);
            dbContext.Warehouses.RemoveRange(dbContext.Warehouses);

            await dbContext.SaveChangesAsync();
        }

        private sealed record InventorySeed(Product Product, Location Location);
    }
}
