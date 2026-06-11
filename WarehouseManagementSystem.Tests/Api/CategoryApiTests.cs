using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;
using WarehouseManagementSystem.Tests.Infrastructure;
using WarehouseManagementSystem.Web.Dtos;
using Xunit;

namespace WarehouseManagementSystem.Tests.Api
{
    public class CategoryApiTests : IClassFixture<WarehouseManagementSystemWebApplicationFactory>
    {
        private readonly WarehouseManagementSystemWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public CategoryApiTests(WarehouseManagementSystemWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnCategories()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Categories.AddRange(
                new Category { Name = "Electronics", Description = "Electronic products" },
                new Category { Name = "Office", Description = "Office products" }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/categories");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();

            categories.Should().NotBeNull();
            categories!.Should().HaveCount(2);
            categories.Select(c => c.Name).Should().Contain("Electronics");
            categories.Select(c => c.Name).Should().Contain("Office");
        }

        [Fact]
        public async Task Get_ShouldReturnFilteredCategories_WhenQueryIsProvided()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Categories.AddRange(
                new Category { Name = "Electronics", Description = "Electronic products" },
                new Category { Name = "Office", Description = "Office products" }
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/categories?query=elect");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();

            categories.Should().NotBeNull();
            categories!.Should().ContainSingle();
            categories[0].Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetById_ShouldReturnCategory_WhenCategoryExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var category = new Category
            {
                Name = "Test Category",
                Description = "Category created for integration test"
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/categories/{category.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<CategoryDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(category.Id);
            dto.Name.Should().Be(category.Name);
            dto.Description.Should().Be(category.Description);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.GetAsync("/api/categories/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldCreateCategory_WhenModelIsValid()
        {
            await ClearDatabaseAsync();

            var createDto = new CategoryCreateDto
            {
                Name = "New Category",
                Description = "Created from test"
            };

            var response = await _client.PostAsJsonAsync("/api/categories", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dto = await response.Content.ReadFromJsonAsync<CategoryDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().BeGreaterThan(0);
            dto.Name.Should().Be(createDto.Name);
            dto.Description.Should().Be(createDto.Description);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Categories.Should().Contain(c => c.Name == "New Category");
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            await ClearDatabaseAsync();

            var invalidDto = new CategoryCreateDto
            {
                Name = "",
                Description = "Invalid category without name"
            };

            var response = await _client.PostAsJsonAsync("/api/categories", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldUpdateCategory_WhenCategoryExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var category = new Category
            {
                Name = "Old Name",
                Description = "Old description"
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            var updateDto = new CategoryUpdateDto
            {
                Name = "Updated Name",
                Description = "Updated description"
            };

            var response = await _client.PutAsJsonAsync($"/api/categories/{category.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<CategoryDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(category.Id);
            dto.Name.Should().Be(updateDto.Name);
            dto.Description.Should().Be(updateDto.Description);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var updatedCategory = await assertDbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == category.Id);

            updatedCategory!.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            await ClearDatabaseAsync();

            var updateDto = new CategoryUpdateDto
            {
                Name = "Updated Name",
                Description = "Updated description"
            };

            var response = await _client.PutAsJsonAsync("/api/categories/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldRemoveCategory_WhenCategoryExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var category = new Category
            {
                Name = "Category To Delete",
                Description = "This category should be deleted"
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/categories/{category.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var deletedCategory = await assertDbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == category.Id);

            deletedCategory.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.DeleteAsync("/api/categories/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnConflict_WhenCategoryHasProducts()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var category = new Category
            {
                Name = "Category With Products",
                Description = "Cannot be deleted"
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                Description = "Product related to category",
                Price = 10,
                Weight = 2,
                ProductReceivedAt = DateTime.UtcNow,
                CategoryId = category.Id
            };

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/categories/{category.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            var existingCategory = await dbContext.Categories.FindAsync(category.Id);
            existingCategory.Should().NotBeNull();
        }

        private async Task ClearDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.Products.RemoveRange(dbContext.Products);
            dbContext.Categories.RemoveRange(dbContext.Categories);

            await dbContext.SaveChangesAsync();
        }

    }
}
