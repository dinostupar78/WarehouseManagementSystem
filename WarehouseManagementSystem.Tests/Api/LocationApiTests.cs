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
    public class LocationApiTests : IClassFixture<WarehouseManagementSystemWebApplicationFactory>
    {
        private readonly WarehouseManagementSystemWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public LocationApiTests(WarehouseManagementSystemWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnLocations()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var warehouse = await CreateWarehouseAsync(dbContext);

            dbContext.Locations.AddRange(
                new Location
                {
                    Code = "MDC-A-01",
                    Zone = "A",
                    ShelfNumber = 1,
                    WarehouseId = warehouse.Id
                },
                new Location
                {
                    Code = "MDC-B-03",
                    Zone = "B",
                    ShelfNumber = 3,
                    WarehouseId = warehouse.Id
                }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/locations");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var locations = await response.Content.ReadFromJsonAsync<List<LocationDto>>();

            locations.Should().NotBeNull();
            locations!.Should().HaveCount(2);
            locations.Select(l => l.Code).Should().Contain("MDC-A-01");
            locations.Select(l => l.Code).Should().Contain("MDC-B-03");
        }

        [Fact]
        public async Task Get_ShouldReturnFilteredLocations_WhenQueryIsProvided()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var warehouse = await CreateWarehouseAsync(dbContext);

            dbContext.Locations.AddRange(
                new Location
                {
                    Code = "MDC-A-01",
                    Zone = "A",
                    ShelfNumber = 1,
                    WarehouseId = warehouse.Id
                },
                new Location
                {
                    Code = "MDC-B-03",
                    Zone = "B",
                    ShelfNumber = 3,
                    WarehouseId = warehouse.Id
                }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/locations?query=b-03");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var locations = await response.Content.ReadFromJsonAsync<List<LocationDto>>();

            locations.Should().NotBeNull();
            locations!.Should().ContainSingle();
            locations[0].Code.Should().Be("MDC-B-03");
        }

        [Fact]
        public async Task GetById_ShouldReturnLocation_WhenLocationExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var warehouse = await CreateWarehouseAsync(dbContext);

            var location = new Location
            {
                Code = "TEST-A-01",
                Zone = "A",
                ShelfNumber = 1,
                WarehouseId = warehouse.Id
            };

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/locations/{location.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<LocationDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(location.Id);
            dto.Code.Should().Be(location.Code);
            dto.Zone.Should().Be(location.Zone);
            dto.ShelfNumber.Should().Be(location.ShelfNumber);
            dto.Warehouse.Should().NotBeNull();
            dto.Warehouse!.Id.Should().Be(warehouse.Id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenLocationDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.GetAsync("/api/locations/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldCreateLocation_WhenModelIsValid()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var warehouse = await CreateWarehouseAsync(dbContext);

            var createDto = new LocationCreateDto
            {
                Code = "NEW-C-05",
                Zone = "C",
                ShelfNumber = 5,
                WarehouseId = warehouse.Id
            };

            var response = await _client.PostAsJsonAsync("/api/locations", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dto = await response.Content.ReadFromJsonAsync<LocationDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().BeGreaterThan(0);
            dto.Code.Should().Be(createDto.Code);
            dto.Zone.Should().Be(createDto.Zone);
            dto.ShelfNumber.Should().Be(createDto.ShelfNumber);
            dto.Warehouse.Should().NotBeNull();
            dto.Warehouse!.Id.Should().Be(warehouse.Id);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            assertDbContext.Locations.Should().Contain(l => l.Code == "NEW-C-05");
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            await ClearDatabaseAsync();

            var invalidDto = new LocationCreateDto
            {
                Code = "",
                Zone = "",
                ShelfNumber = 0,
                WarehouseId = 0
            };

            var response = await _client.PostAsJsonAsync("/api/locations", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenWarehouseDoesNotExist()
        {
            await ClearDatabaseAsync();

            var createDto = new LocationCreateDto
            {
                Code = "BAD-WH-01",
                Zone = "A",
                ShelfNumber = 1,
                WarehouseId = 999
            };

            var response = await _client.PostAsJsonAsync("/api/locations", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldUpdateLocation_WhenLocationExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var warehouse = await CreateWarehouseAsync(dbContext);

            var location = new Location
            {
                Code = "OLD-A-01",
                Zone = "A",
                ShelfNumber = 1,
                WarehouseId = warehouse.Id
            };

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            var updateDto = new LocationUpdateDto
            {
                Code = "UPDATED-B-02",
                Zone = "B",
                ShelfNumber = 2,
                WarehouseId = warehouse.Id
            };

            var response = await _client.PutAsJsonAsync($"/api/locations/{location.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<LocationDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(location.Id);
            dto.Code.Should().Be(updateDto.Code);
            dto.Zone.Should().Be(updateDto.Zone);
            dto.ShelfNumber.Should().Be(updateDto.ShelfNumber);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var updatedLocation = await assertDbContext.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == location.Id);

            updatedLocation!.Code.Should().Be("UPDATED-B-02");
            updatedLocation.Zone.Should().Be("B");
            updatedLocation.ShelfNumber.Should().Be(2);
        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenLocationDoesNotExist()
        {
            await ClearDatabaseAsync();

            var updateDto = new LocationUpdateDto
            {
                Code = "UPDATED-B-02",
                Zone = "B",
                ShelfNumber = 2,
                WarehouseId = 1
            };

            var response = await _client.PutAsJsonAsync("/api/locations/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenWarehouseDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var warehouse = await CreateWarehouseAsync(dbContext);

            var location = new Location
            {
                Code = "LOC-A-01",
                Zone = "A",
                ShelfNumber = 1,
                WarehouseId = warehouse.Id
            };

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            var updateDto = new LocationUpdateDto
            {
                Code = "UPDATED-B-02",
                Zone = "B",
                ShelfNumber = 2,
                WarehouseId = 999
            };

            var response = await _client.PutAsJsonAsync($"/api/locations/{location.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_ShouldRemoveLocation_WhenLocationExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var warehouse = await CreateWarehouseAsync(dbContext);

            var location = new Location
            {
                Code = "DELETE-A-01",
                Zone = "A",
                ShelfNumber = 1,
                WarehouseId = warehouse.Id
            };

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/locations/{location.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var deletedLocation = await assertDbContext.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == location.Id);

            deletedLocation.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenLocationDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.DeleteAsync("/api/locations/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private async Task<Warehouse> CreateWarehouseAsync(WarehouseManagementSystemDbContext dbContext)
        {
            var warehouse = new Warehouse
            {
                Name = "Test Warehouse",
                Address = "100 Test Street",
                City = "Zagreb",
                Country = "Croatia",
                Capacity = 5000
            };

            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync();

            return warehouse;
        }

        private async Task ClearDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Inventories.RemoveRange(dbContext.Inventories);
            dbContext.Locations.RemoveRange(dbContext.Locations);
            dbContext.Warehouses.RemoveRange(dbContext.Warehouses);

            await dbContext.SaveChangesAsync();
        }
    }
}
