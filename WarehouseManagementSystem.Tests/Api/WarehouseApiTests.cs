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
    public class WarehouseApiTests : IClassFixture<WarehouseManagementSystemWebApplicationFactory>
    {
        private readonly WarehouseManagementSystemWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public WarehouseApiTests(WarehouseManagementSystemWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnWarehouses()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Warehouses.AddRange(
                new Warehouse
                {
                    Name = "Main Distribution Center",
                    Address = "100 Logistics Street",
                    City = "Zagreb",
                    Country = "Croatia",
                    Capacity = 5000
                },
                new Warehouse
                {
                    Name = "Eastern Fulfillment Hub",
                    Address = "200 Storage Avenue",
                    City = "Osijek",
                    Country = "Croatia",
                    Capacity = 2500
                }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/warehouses");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var warehouses = await response.Content.ReadFromJsonAsync<List<WarehouseDto>>();

            warehouses.Should().NotBeNull();
            warehouses!.Should().HaveCount(2);
            warehouses.Select(w => w.Name).Should().Contain("Main Distribution Center");
            warehouses.Select(w => w.Name).Should().Contain("Eastern Fulfillment Hub");
        }

        [Fact]
        public async Task Get_ShouldReturnFilteredWarehouses_WhenQueryIsProvided()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Warehouses.AddRange(
                new Warehouse
                {
                    Name = "Main Distribution Center",
                    Address = "100 Logistics Street",
                    City = "Zagreb",
                    Country = "Croatia",
                    Capacity = 5000
                },
                new Warehouse
                {
                    Name = "Eastern Fulfillment Hub",
                    Address = "200 Storage Avenue",
                    City = "Osijek",
                    Country = "Croatia",
                    Capacity = 2500
                }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/warehouses?query=osijek");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var warehouses = await response.Content.ReadFromJsonAsync<List<WarehouseDto>>();

            warehouses.Should().NotBeNull();
            warehouses!.Should().ContainSingle();
            warehouses[0].Name.Should().Be("Eastern Fulfillment Hub");
        }

        [Fact]
        public async Task GetById_ShouldReturnWarehouse_WhenWarehouseExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var warehouse = new Warehouse
            {
                Name = "Test Warehouse",
                Address = "Test Address 1",
                City = "Split",
                Country = "Croatia",
                Capacity = 1000
            };

            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/warehouses/{warehouse.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<WarehouseDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(warehouse.Id);
            dto.Name.Should().Be(warehouse.Name);
            dto.Address.Should().Be(warehouse.Address);
            dto.City.Should().Be(warehouse.City);
            dto.Country.Should().Be(warehouse.Country);
            dto.Capacity.Should().Be(warehouse.Capacity);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenWarehouseDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.GetAsync("/api/warehouses/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldCreateWarehouse_WhenModelIsValid()
        {
            await ClearDatabaseAsync();

            var createDto = new WarehouseCreateDto
            {
                Name = "New Warehouse",
                Address = "300 Warehouse Road",
                City = "Rijeka",
                Country = "Croatia",
                Capacity = 3000
            };

            var response = await _client.PostAsJsonAsync("/api/warehouses", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dto = await response.Content.ReadFromJsonAsync<WarehouseDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().BeGreaterThan(0);
            dto.Name.Should().Be(createDto.Name);
            dto.Address.Should().Be(createDto.Address);
            dto.City.Should().Be(createDto.City);
            dto.Country.Should().Be(createDto.Country);
            dto.Capacity.Should().Be(createDto.Capacity);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Warehouses.Should().Contain(w => w.Name == "New Warehouse");
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            await ClearDatabaseAsync();

            var invalidDto = new WarehouseCreateDto
            {
                Name = "",
                Address = "Invalid Address",
                City = "Zagreb",
                Country = "Croatia",
                Capacity = 0
            };

            var response = await _client.PostAsJsonAsync("/api/warehouses", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldUpdateWarehouse_WhenWarehouseExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var warehouse = new Warehouse
            {
                Name = "Old Warehouse",
                Address = "Old Address",
                City = "Old City",
                Country = "Croatia",
                Capacity = 1000
            };

            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync();

            var updateDto = new WarehouseUpdateDto
            {
                Name = "Updated Warehouse",
                Address = "Updated Address",
                City = "Updated City",
                Country = "Croatia",
                Capacity = 4000
            };

            var response = await _client.PutAsJsonAsync($"/api/warehouses/{warehouse.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<WarehouseDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(warehouse.Id);
            dto.Name.Should().Be(updateDto.Name);
            dto.Address.Should().Be(updateDto.Address);
            dto.City.Should().Be(updateDto.City);
            dto.Country.Should().Be(updateDto.Country);
            dto.Capacity.Should().Be(updateDto.Capacity);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var updatedWarehouse = await assertDbContext.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == warehouse.Id);

            updatedWarehouse!.Name.Should().Be("Updated Warehouse");
            updatedWarehouse.Capacity.Should().Be(4000);
        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenWarehouseDoesNotExist()
        {
            await ClearDatabaseAsync();

            var updateDto = new WarehouseUpdateDto
            {
                Name = "Updated Warehouse",
                Address = "Updated Address",
                City = "Updated City",
                Country = "Croatia",
                Capacity = 4000
            };

            var response = await _client.PutAsJsonAsync("/api/warehouses/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldRemoveWarehouse_WhenWarehouseExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var warehouse = new Warehouse
            {
                Name = "Warehouse To Delete",
                Address = "Delete Address",
                City = "Pula",
                Country = "Croatia",
                Capacity = 1200
            };

            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/warehouses/{warehouse.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var deletedWarehouse = await assertDbContext.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == warehouse.Id);

            deletedWarehouse.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenWarehouseDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.DeleteAsync("/api/warehouses/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnConflict_WhenWarehouseHasPurchaseOrders()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                ContactEmail = "supplier@test.com",
                ContactPhone = "+385911234567",
                ContactAddress = "Supplier Address"
            };

            var warehouse = new Warehouse
            {
                Name = "Warehouse With Orders",
                Address = "Order Address",
                City = "Zagreb",
                Country = "Croatia",
                Capacity = 2000
            };

            dbContext.Suppliers.Add(supplier);
            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync();

            var purchaseOrder = new PurchaseOrder
            {
                OrderNumber = 1,
                OrderDate = DateTime.UtcNow,
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
                TotalAmount = 100,
                Status = OrderStatus.Pending,
                SupplierId = supplier.Id,
                WarehouseId = warehouse.Id
            };

            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/warehouses/{warehouse.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            var existingWarehouse = await dbContext.Warehouses.FindAsync(warehouse.Id);
            existingWarehouse.Should().NotBeNull();
        }

        private async Task ClearDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.PurchaseOrders.RemoveRange(dbContext.PurchaseOrders);
            dbContext.Suppliers.RemoveRange(dbContext.Suppliers);
            dbContext.Locations.RemoveRange(dbContext.Locations);
            dbContext.Warehouses.RemoveRange(dbContext.Warehouses);

            await dbContext.SaveChangesAsync();
        }
    }
}
