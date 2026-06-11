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
    public class SupplierApiTests : IClassFixture<WarehouseManagementSystemWebApplicationFactory>
    {
        private readonly WarehouseManagementSystemWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public SupplierApiTests(WarehouseManagementSystemWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnSuppliers()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Suppliers.AddRange(
                CreateSupplier("Global Supplies", "Ana Horvat", "ana@supplies.test", "+385911111111", "Supply Street 1"),
                CreateSupplier("Warehouse Partners", "Marko Ilic", "marko@partners.test", "+385922222222", "Partner Avenue 2")
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/suppliers");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var suppliers = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();

            suppliers.Should().NotBeNull();
            suppliers!.Should().HaveCount(2);
            suppliers.Select(s => s.Name).Should().Contain("Global Supplies");
            suppliers.Select(s => s.Name).Should().Contain("Warehouse Partners");
        }

        [Fact]
        public async Task Get_ShouldReturnFilteredSuppliers_WhenQueryIsProvided()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Suppliers.AddRange(
                CreateSupplier("Global Supplies", "Ana Horvat", "ana@supplies.test", "+385911111111", "Supply Street 1"),
                CreateSupplier("Warehouse Partners", "Marko Ilic", "marko@partners.test", "+385922222222", "Partner Avenue 2")
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/suppliers?query=marko");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var suppliers = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();

            suppliers.Should().NotBeNull();
            suppliers!.Should().ContainSingle();
            suppliers[0].Name.Should().Be("Warehouse Partners");
        }

        [Fact]
        public async Task GetById_ShouldReturnSupplier_WhenSupplierExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var supplier = CreateSupplier("Test Supplier", "Test Person", "test@supplier.test", "+385933333333", "Test Address");

            dbContext.Suppliers.Add(supplier);
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/suppliers/{supplier.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<SupplierDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(supplier.Id);
            dto.Name.Should().Be(supplier.Name);
            dto.ContactPerson.Should().Be(supplier.ContactPerson);
            dto.ContactEmail.Should().Be(supplier.ContactEmail);
            dto.ContactPhone.Should().Be(supplier.ContactPhone);
            dto.ContactAddress.Should().Be(supplier.ContactAddress);
            dto.PurchaseOrderCount.Should().Be(0);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenSupplierDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.GetAsync("/api/suppliers/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldCreateSupplier_WhenModelIsValid()
        {
            await ClearDatabaseAsync();

            var createDto = new SupplierCreateDto
            {
                Name = "New Supplier",
                ContactPerson = "New Contact",
                ContactEmail = "new@supplier.test",
                ContactPhone = "+385944444444",
                ContactAddress = "New Supplier Address"
            };

            var response = await _client.PostAsJsonAsync("/api/suppliers", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dto = await response.Content.ReadFromJsonAsync<SupplierDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().BeGreaterThan(0);
            dto.Name.Should().Be(createDto.Name);
            dto.ContactPerson.Should().Be(createDto.ContactPerson);
            dto.ContactEmail.Should().Be(createDto.ContactEmail);
            dto.ContactPhone.Should().Be(createDto.ContactPhone);
            dto.ContactAddress.Should().Be(createDto.ContactAddress);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Suppliers.Should().Contain(s => s.Name == "New Supplier");
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            await ClearDatabaseAsync();

            var invalidDto = new SupplierCreateDto
            {
                Name = "",
                ContactPerson = "",
                ContactEmail = "not-an-email",
                ContactPhone = "",
                ContactAddress = ""
            };

            var response = await _client.PostAsJsonAsync("/api/suppliers", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldUpdateSupplier_WhenSupplierExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var supplier = CreateSupplier("Old Supplier", "Old Contact", "old@supplier.test", "+385955555555", "Old Address");

            dbContext.Suppliers.Add(supplier);
            await dbContext.SaveChangesAsync();

            var updateDto = new SupplierUpdateDto
            {
                Name = "Updated Supplier",
                ContactPerson = "Updated Contact",
                ContactEmail = "updated@supplier.test",
                ContactPhone = "+385966666666",
                ContactAddress = "Updated Address"
            };

            var response = await _client.PutAsJsonAsync($"/api/suppliers/{supplier.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<SupplierDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(supplier.Id);
            dto.Name.Should().Be(updateDto.Name);
            dto.ContactPerson.Should().Be(updateDto.ContactPerson);
            dto.ContactEmail.Should().Be(updateDto.ContactEmail);
            dto.ContactPhone.Should().Be(updateDto.ContactPhone);
            dto.ContactAddress.Should().Be(updateDto.ContactAddress);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var updatedSupplier = await assertDbContext.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == supplier.Id);

            updatedSupplier!.Name.Should().Be("Updated Supplier");
            updatedSupplier.ContactEmail.Should().Be("updated@supplier.test");
        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenSupplierDoesNotExist()
        {
            await ClearDatabaseAsync();

            var updateDto = new SupplierUpdateDto
            {
                Name = "Updated Supplier",
                ContactPerson = "Updated Contact",
                ContactEmail = "updated@supplier.test",
                ContactPhone = "+385966666666",
                ContactAddress = "Updated Address"
            };

            var response = await _client.PutAsJsonAsync("/api/suppliers/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldRemoveSupplier_WhenSupplierExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var supplier = CreateSupplier("Supplier To Delete", "Delete Contact", "delete@supplier.test", "+385977777777", "Delete Address");

            dbContext.Suppliers.Add(supplier);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/suppliers/{supplier.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var deletedSupplier = await assertDbContext.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == supplier.Id);

            deletedSupplier.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenSupplierDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.DeleteAsync("/api/suppliers/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnConflict_WhenSupplierHasPurchaseOrders()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var supplier = CreateSupplier("Supplier With Orders", "Order Contact", "orders@supplier.test", "+385988888888", "Order Address");
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

            var purchaseOrder = new PurchaseOrder
            {
                OrderNumber = 1,
                OrderDate = DateTime.UtcNow,
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
                TotalAmount = 150,
                Status = OrderStatus.Pending,
                SupplierId = supplier.Id,
                WarehouseId = warehouse.Id
            };

            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/suppliers/{supplier.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            var existingSupplier = await dbContext.Suppliers.FindAsync(supplier.Id);
            existingSupplier.Should().NotBeNull();
        }

        private static Supplier CreateSupplier(
            string name,
            string contactPerson,
            string contactEmail,
            string contactPhone,
            string contactAddress)
        {
            return new Supplier
            {
                Name = name,
                ContactPerson = contactPerson,
                ContactEmail = contactEmail,
                ContactPhone = contactPhone,
                ContactAddress = contactAddress
            };
        }

        private async Task ClearDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.PurchaseOrders.RemoveRange(dbContext.PurchaseOrders);
            dbContext.Suppliers.RemoveRange(dbContext.Suppliers);
            dbContext.Warehouses.RemoveRange(dbContext.Warehouses);

            await dbContext.SaveChangesAsync();
        }
    }
}
