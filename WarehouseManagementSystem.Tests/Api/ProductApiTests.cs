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
    public class ProductApiTests : IClassFixture<WarehouseManagementSystemWebApplicationFactory>
    {
        private readonly WarehouseManagementSystemWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ProductApiTests(WarehouseManagementSystemWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnProducts()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var category = await CreateCategoryAsync(dbContext);

            dbContext.Products.AddRange(
                CreateProduct("Barcode Scanner", "Scanner for warehouse operations", 120, 1.5m, category.Id),
                CreateProduct("Storage Box", "Reusable storage box", 15, 0.8m, category.Id)
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/products");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

            products.Should().NotBeNull();
            products!.Should().HaveCount(2);
            products.Select(p => p.Name).Should().Contain("Barcode Scanner");
            products.Select(p => p.Name).Should().Contain("Storage Box");
        }

        [Fact]
        public async Task Get_ShouldReturnFilteredProducts_WhenQueryIsProvided()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var category = await CreateCategoryAsync(dbContext);

            dbContext.Products.AddRange(
                CreateProduct("Barcode Scanner", "Scanner for warehouse operations", 120, 1.5m, category.Id),
                CreateProduct("Storage Box", "Reusable storage box", 15, 0.8m, category.Id)
            );

            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/api/products?query=scanner");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

            products.Should().NotBeNull();
            products!.Should().ContainSingle();
            products[0].Name.Should().Be("Barcode Scanner");
        }

        [Fact]
        public async Task GetById_ShouldReturnProduct_WhenProductExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var category = await CreateCategoryAsync(dbContext);

            var product = CreateProduct("Test Product", "Product for get by id test", 50, 2, category.Id);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/products/{product.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<ProductDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(product.Id);
            dto.Name.Should().Be(product.Name);
            dto.Description.Should().Be(product.Description);
            dto.Price.Should().Be(product.Price);
            dto.Weight.Should().Be(product.Weight);
            dto.ProductReceivedAt.Should().Be(product.ProductReceivedAt);
            dto.Category.Should().NotBeNull();
            dto.Category.Id.Should().Be(category.Id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.GetAsync("/api/products/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ShouldCreateProduct_WhenModelIsValid()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var category = await CreateCategoryAsync(dbContext);
            var receivedAt = new DateTime(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);

            var createDto = new ProductCreateDto
            {
                Name = "New Product",
                Description = "Created from product API test",
                Price = 99.99m,
                Weight = 4.5m,
                ProductReceivedAt = receivedAt,
                CategoryId = category.Id
            };

            var response = await _client.PostAsJsonAsync("/api/products", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dto = await response.Content.ReadFromJsonAsync<ProductDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().BeGreaterThan(0);
            dto.Name.Should().Be(createDto.Name);
            dto.Description.Should().Be(createDto.Description);
            dto.Price.Should().Be(createDto.Price);
            dto.Weight.Should().Be(createDto.Weight);
            dto.ProductReceivedAt.Should().Be(receivedAt);
            dto.Category.Should().NotBeNull();
            dto.Category.Id.Should().Be(category.Id);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            assertDbContext.Products.Should().Contain(p => p.Name == "New Product");
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            await ClearDatabaseAsync();

            var invalidDto = new ProductCreateDto
            {
                Name = "",
                Description = "Invalid product",
                Price = 0,
                Weight = 0,
                ProductReceivedAt = new DateTime(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc),
                CategoryId = 0
            };

            var response = await _client.PostAsJsonAsync("/api/products", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenCategoryDoesNotExist()
        {
            await ClearDatabaseAsync();

            var createDto = new ProductCreateDto
            {
                Name = "Product Without Category",
                Description = "Should fail because category is missing",
                Price = 20,
                Weight = 2,
                ProductReceivedAt = new DateTime(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc),
                CategoryId = 999
            };

            var response = await _client.PostAsJsonAsync("/api/products", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_ShouldUpdateProduct_WhenProductExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var category = await CreateCategoryAsync(dbContext);

            var product = CreateProduct("Old Product", "Old description", 10, 1, category.Id);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var receivedAt = new DateTime(2026, 1, 16, 10, 0, 0, DateTimeKind.Utc);
            var updateDto = new ProductUpdateDto
            {
                Name = "Updated Product",
                Description = "Updated description",
                Price = 150,
                Weight = 7.5m,
                ProductReceivedAt = receivedAt,
                CategoryId = category.Id
            };

            var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<ProductDto>();

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(product.Id);
            dto.Name.Should().Be(updateDto.Name);
            dto.Description.Should().Be(updateDto.Description);
            dto.Price.Should().Be(updateDto.Price);
            dto.Weight.Should().Be(updateDto.Weight);
            dto.ProductReceivedAt.Should().Be(receivedAt);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var updatedProduct = await assertDbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            updatedProduct!.Name.Should().Be("Updated Product");
            updatedProduct.Price.Should().Be(150);
        }

        [Fact]
        public async Task Put_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            await ClearDatabaseAsync();

            var updateDto = new ProductUpdateDto
            {
                Name = "Updated Product",
                Description = "Updated description",
                Price = 150,
                Weight = 7.5m,
                ProductReceivedAt = new DateTime(2026, 1, 16, 10, 0, 0, DateTimeKind.Utc),
                CategoryId = 1
            };

            var response = await _client.PutAsJsonAsync("/api/products/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Put_ShouldReturnBadRequest_WhenCategoryDoesNotExist()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var category = await CreateCategoryAsync(dbContext);

            var product = CreateProduct("Product To Update", "Description", 10, 1, category.Id);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var updateDto = new ProductUpdateDto
            {
                Name = "Updated Product",
                Description = "Updated description",
                Price = 150,
                Weight = 7.5m,
                ProductReceivedAt = new DateTime(2026, 1, 16, 10, 0, 0, DateTimeKind.Utc),
                CategoryId = 999
            };

            var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_ShouldRemoveProduct_WhenProductExists()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var category = await CreateCategoryAsync(dbContext);

            var product = CreateProduct("Product To Delete", "Should be deleted", 30, 2, category.Id);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/products/{product.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var assertScope = _factory.Services.CreateScope();
            var assertDbContext = assertScope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            var deletedProduct = await assertDbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            deletedProduct.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            await ClearDatabaseAsync();

            var response = await _client.DeleteAsync("/api/products/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnConflict_WhenProductHasPurchaseOrderItems()
        {
            await ClearDatabaseAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();
            var category = await CreateCategoryAsync(dbContext);

            var product = CreateProduct("Product With Order Item", "Cannot be deleted", 55, 3, category.Id);
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Contact",
                ContactEmail = "supplier@product.test",
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

            dbContext.Products.Add(product);
            dbContext.Suppliers.Add(supplier);
            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync();

            var purchaseOrder = new PurchaseOrder
            {
                OrderNumber = 1,
                OrderDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc),
                ExpectedDeliveryDate = new DateTime(2026, 1, 17, 8, 0, 0, DateTimeKind.Utc),
                TotalAmount = 55,
                Status = OrderStatus.Pending,
                SupplierId = supplier.Id,
                WarehouseId = warehouse.Id
            };

            dbContext.PurchaseOrders.Add(purchaseOrder);
            await dbContext.SaveChangesAsync();

            dbContext.PurchaseOrderItems.Add(new PurchaseOrderItem
            {
                Quantity = 1,
                UnitPrice = 55,
                ProductId = product.Id,
                PurchaseOrderId = purchaseOrder.Id
            });

            await dbContext.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/products/{product.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            var existingProduct = await dbContext.Products.FindAsync(product.Id);
            existingProduct.Should().NotBeNull();
        }

        private async Task<Category> CreateCategoryAsync(WarehouseManagementSystemDbContext dbContext)
        {
            var category = new Category
            {
                Name = "Test Category",
                Description = "Category for product tests"
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            return category;
        }

        private static Product CreateProduct(
            string name,
            string description,
            decimal price,
            decimal weight,
            int categoryId)
        {
            return new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Weight = weight,
                ProductReceivedAt = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                CategoryId = categoryId
            };
        }

        private async Task ClearDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseManagementSystemDbContext>();

            dbContext.PurchaseOrderItems.RemoveRange(dbContext.PurchaseOrderItems);
            dbContext.PurchaseOrders.RemoveRange(dbContext.PurchaseOrders);
            dbContext.Inventories.RemoveRange(dbContext.Inventories);
            dbContext.Products.RemoveRange(dbContext.Products);
            dbContext.Suppliers.RemoveRange(dbContext.Suppliers);
            dbContext.Warehouses.RemoveRange(dbContext.Warehouses);
            dbContext.Categories.RemoveRange(dbContext.Categories);

            await dbContext.SaveChangesAsync();
        }
    }
}
