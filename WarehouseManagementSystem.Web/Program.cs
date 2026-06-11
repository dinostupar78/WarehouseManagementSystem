using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WarehouseManagementSystem.DAL.Data;
using WarehouseManagementSystem.Model;
using WarehouseManagementSystem.Web.Repositories;
using WarehouseManagementSystem.Web.Services;

public partial class Program
{
    public static void Main(string[] args)
    {
        if (args.Any(a => a.Equals("--console", StringComparison.OrdinalIgnoreCase)))
        {
            RunConsoleApp();
            return;
        }

        RunWebApp(args);
    }

    private static void RunWebApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Dodaj DbContext sa PostgreSQL
        var connectionString = builder.Configuration.GetConnectionString("WarehouseManagementSystemDbContext");

        builder.Services.AddDbContext<WarehouseManagementSystemDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddDefaultIdentity<AppUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<WarehouseManagementSystemDbContext>();

        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            });

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, NoOpEmailSender>();

        // builder.Services.AddSingleton<WarehouseMockRepository>();
        // builder.Services.AddSingleton<LocationMockRepository>();
        // builder.Services.AddSingleton<CategoryMockRepository>();
        // builder.Services.AddSingleton<ProductMockRepository>();
        // builder.Services.AddSingleton<InventoryMockRepository>();
        // builder.Services.AddSingleton<SupplierMockRepository>();
        // builder.Services.AddSingleton<PurchaseOrderMockRepository>();
        // builder.Services.AddSingleton<PurchaseOrderItemMockRepository>();

        builder.Services.AddScoped<WarehouseRepository>();
        builder.Services.AddScoped<LocationRepository>();
        builder.Services.AddScoped<CategoryRepository>();
        builder.Services.AddScoped<ProductRepository>();
        builder.Services.AddScoped<InventoryRepository>();
        builder.Services.AddScoped<SupplierRepository>();
        builder.Services.AddScoped<PurchaseOrderRepository>();
        builder.Services.AddScoped<PurchaseOrderItemRepository>();

        var app = builder.Build();
        SeedIdentityRoles(app.Services).GetAwaiter().GetResult();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days.
            app.UseHsts();
        }

        var supportedCultures = new[]
        {
            new CultureInfo("hr-HR"),
            new CultureInfo("en-US")
        };

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("hr-HR"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Welcome}/{action=Index}/{id?}");

        app.MapRazorPages();

        app.Run();
    }

    private static async Task SeedIdentityRoles(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Admin", "Operator", "Guest" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static void RunConsoleApp()
    {
        var warehouses = new List<Warehouse>
        {
            new Warehouse
            {
                Id = 1,
                Name = "Main Distribution Center",
                Address = "1250 Logistics Parkway",
                City = "Chicago",
                Country = "USA",
                Capacity = 1000
            },
            new Warehouse
            {
                Id = 2,
                Name = "Eastern Fulfillment Hub",
                Address = "840 Industrial Avenue",
                City = "Columbus",
                Country = "USA",
                Capacity = 750
            },
            new Warehouse
            {
                Id = 3,
                Name = "Western Logistics Center",
                Address = "620 Commerce Drive",
                City = "Phoenix",
                Country = "USA",
                Capacity = 500
            }
        };

        var locations = new List<Location>
        {
            new Location
            {
                Id = 1,
                Code = "MDC-A-01",
                Zone = "A",
                ShelfNumber = 1,
                Warehouse = warehouses[0],
                WarehouseId = warehouses[0].Id,
            },
            new Location
            {
                Id = 2,
                Code = "MDC-B-03",
                Zone = "B",
                ShelfNumber = 3,
                Warehouse = warehouses[0],
                WarehouseId = warehouses[0].Id,
            },
            new Location
            {
                Id = 3,
                Code = "EFH-A-01",
                Zone = "A",
                ShelfNumber = 1,
                Warehouse = warehouses[1],
                WarehouseId = warehouses[1].Id,
            },
            new Location
            {
                Id = 4,
                Code = "EFH-C-06",
                Zone = "C",
                ShelfNumber = 6,
                Warehouse = warehouses[1],
                WarehouseId = warehouses[1].Id,
            }
        };

        foreach (var warehouse in warehouses)
        {
            foreach (var location in locations.Where(location => location.WarehouseId == warehouse.Id))
            {
                warehouse.Locations.Add(location);
            }
        }

        var categories = new List<Category>
        {
            new Category
            {
                Id = 1,
                Name = "Electronics",
                Description = "Electronic devices and accessories"
            },
            new Category
            {
                Id = 2,
                Name = "Furniture",
                Description = "Warehouse furniture and storage solutions"
            },
            new Category
            {
                Id = 3,
                Name = "Office Supplies",
                Description = "General office supplies and equipment"
            }
        };

        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Handheld Barcode Scanner",
                Description = "Portable barcode scanner designed for fast and accurate inventory processing",
                Price = 120.00m,
                Weight = 0.35m,
                ProductReceivedAt = new DateTime(2026, 01, 06),
                Category = categories[2],
                CategoryId = categories[2].Id
            },
            new Product
            {
                Id = 2,
                Name = "Industrial Label Printer",
                Description = "High-speed thermal printer for warehouse and shipping labels",
                Price = 230m,
                Weight = 2.10m,
                ProductReceivedAt = new DateTime(2026, 02, 15),
                Category = categories[2],
                CategoryId = categories[2].Id
            },
            new Product
            {
                Id = 3,
                Name = "Laptop",
                Description = "15-inch laptop built for office productivity and warehouse administration",
                Price = 1500m,
                Weight = 1.5m,
                ProductReceivedAt = new DateTime(2026, 02, 18),
                Category = categories[0],
                CategoryId = categories[0].Id
            },
            new Product
            {
                Id = 4,
                Name = "Monitor",
                Description = "Full HD monitor suitable for administrative and operational workstations",
                Price = 300m,
                Weight = 3.0m,
                ProductReceivedAt = new DateTime(2026, 03, 07),
                Category = categories[0],
                CategoryId = categories[0].Id
            },
            new Product
            {
                Id = 5,
                Name = "Ergonomic Office Chair",
                Description = "Adjustable office chair designed for long-duration seated work",
                Price = 250m,
                Weight = 15m,
                ProductReceivedAt = new DateTime(2026, 03, 18),
                Category = categories[1],
                CategoryId = categories[1].Id
            },
            new Product
            {
                Id = 6,
                Name = "Workstation Desk",
                Description = "Durable desk with ample surface area for office and warehouse coordination tasks",
                Price = 400m,
                Weight = 30m,
                ProductReceivedAt = new DateTime(2026, 03, 13),
                Category = categories[2],
                CategoryId = categories[2].Id
            },
        };

        foreach (var category in categories)
        {
            foreach (var product in products.Where(product => product.CategoryId == category.Id))
            {
                category.Products.Add(product);
            }
        }

        var inventories = new List<Inventory>
        {
            new Inventory
            {
                Id = 1,
                Product = products[0],
                ProductId = products[0].Id,
                Location = locations[0],
                LocationId = locations[0].Id,
                Quantity = 1,
                LastUpdated = new DateTime(2026, 03, 30)
            },
            new Inventory
            {
                Id = 2,
                Product = products[1],
                ProductId = products[1].Id,
                Location = locations[1],
                LocationId = locations[1].Id,
                Quantity = 5,
                LastUpdated = new DateTime(2026, 03, 27)
            },
            new Inventory
            {
                Id = 3,
                Product = products[2],
                ProductId = products[2].Id,
                Location = locations[2],
                LocationId = locations[2].Id,
                Quantity = 25,
                LastUpdated = new DateTime(2026, 03, 23)
            },
            new Inventory
            {
                Id = 4,
                Product = products[3],
                ProductId = products[3].Id,
                Location = locations[3],
                LocationId = locations[3].Id,
                Quantity = 100,
                LastUpdated = new DateTime(2026, 03, 25)
            },
            new Inventory
            {
                Id = 5,
                Product = products[4],
                ProductId = products[4].Id,
                Location = locations[3],
                LocationId = locations[3].Id,
                Quantity = 255,
                LastUpdated = new DateTime(2026, 03, 22)
            },
            new Inventory
            {
                Id = 6,
                Product = products[5],
                ProductId = products[5].Id,
                Location = locations[3],
                LocationId = locations[3].Id,
                Quantity = 500,
                LastUpdated = new DateTime(2026, 03, 26)
            }
        };

        foreach (var inventory in inventories)
        {
            inventory.Product.Inventories.Add(inventory);
            inventory.Location.Inventories.Add(inventory);
        }

        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                Id = 1,
                Name = "AutoID Systems",
                ContactPerson = "Michael Carter",
                ContactEmail = "michael.carter@autoidsystems.com",
                ContactPhone = "+1-312-555-0142",
                ContactAddress = "910 Industrial Park Road, Chicago, USA"
            },
            new Supplier
            {
                Id = 2,
                Name = "TechCore Solutions",
                ContactPerson = "Laura Bennett",
                ContactEmail = "laura.bennett@techcore.com",
                ContactPhone = "+1-614-555-0187",
                ContactAddress = "455 Technology Avenue, Columbus, USA"
            },
            new Supplier
            {
                Id = 3,
                Name = "Office Furnishings Group",
                ContactPerson = "Daniel Foster",
                ContactEmail = "daniel.foster@officefurnishings.com",
                ContactPhone = "+1-602-555-0119",
                ContactAddress = "220 Business Center Drive, Phoenix, USA"
            }
        };

        var purchaseOrders = new List<PurchaseOrder>
        {
            new PurchaseOrder
            {
                Id = 1,
                OrderNumber = 1001,
                OrderDate = new DateTime(2026, 03, 02),
                ExpectedDeliveryDate = new DateTime(2026, 03, 09),
                Status = OrderStatus.Shipped,
                Supplier = suppliers[0],
                SupplierId = suppliers[0].Id,
                Warehouse = warehouses[0],
                WarehouseId = warehouses[0].Id
            },
            new PurchaseOrder
            {
                Id = 2,
                OrderNumber = 1002,
                OrderDate = new DateTime(2026, 03, 08),
                ExpectedDeliveryDate = new DateTime(2026, 03, 16),
                Status = OrderStatus.Shipped,
                Supplier = suppliers[1],
                SupplierId = suppliers[1].Id,
                Warehouse = warehouses[1],
                WarehouseId = warehouses[1].Id
            },
            new PurchaseOrder
            {
                Id = 3,
                OrderNumber = 1003,
                OrderDate = new DateTime(2026, 03, 12),
                ExpectedDeliveryDate = new DateTime(2026, 03, 21),
                Status = OrderStatus.Delivered,
                Supplier = suppliers[2],
                SupplierId = suppliers[2].Id,
                Warehouse = warehouses[2],
                WarehouseId = warehouses[2].Id
            }
        };

        foreach (var purchaseOrder in purchaseOrders)
        {
            purchaseOrder.Supplier.PurchaseOrders.Add(purchaseOrder);
            purchaseOrder.Warehouse.PurchaseOrders.Add(purchaseOrder);
        }

        var orderItems = new List<PurchaseOrderItem>
        {
            new PurchaseOrderItem
            {
                Id = 1,
                PurchaseOrder = purchaseOrders[0],
                PurchaseOrderId = purchaseOrders[0].Id,
                Product = products[0],
                ProductId = products[0].Id,
                Quantity = 50,
                UnitPrice = 120m
            },
            new PurchaseOrderItem
            {
                Id = 2,
                PurchaseOrder = purchaseOrders[0],
                PurchaseOrderId = purchaseOrders[0].Id,
                Product = products[1],
                ProductId = products[1].Id,
                Quantity = 100,
                UnitPrice = 230m
            },
            new PurchaseOrderItem
            {
                Id = 3,
                PurchaseOrder = purchaseOrders[1],
                PurchaseOrderId = purchaseOrders[1].Id,
                Product = products[2],
                ProductId = products[2].Id,
                Quantity = 30,
                UnitPrice = 1500m
            },
            new PurchaseOrderItem
            {
                Id = 4,
                PurchaseOrder = purchaseOrders[1],
                PurchaseOrderId = purchaseOrders[1].Id,
                Product = products[3],
                ProductId = products[3].Id,
                Quantity = 20,
                UnitPrice = 300m
            },
            new PurchaseOrderItem
            {
                Id = 5,
                PurchaseOrder = purchaseOrders[2],
                PurchaseOrderId = purchaseOrders[2].Id,
                Product = products[4],
                ProductId = products[4].Id,
                Quantity = 40,
                UnitPrice = 250m
            },
            new PurchaseOrderItem
            {
                Id = 6,
                PurchaseOrder = purchaseOrders[2],
                PurchaseOrderId = purchaseOrders[2].Id,
                Product = products[5],
                ProductId = products[5].Id,
                Quantity = 25,
                UnitPrice = 400m
            }
        };

        foreach (var orderItem in orderItems)
        {
            orderItem.PurchaseOrder.PurchaseOrderItems.Add(orderItem);
            orderItem.Product.PurchaseOrderItems.Add(orderItem);
        }

        foreach (var order in purchaseOrders)
        {
            order.TotalAmount = order.PurchaseOrderItems.Sum(i => i.Quantity * i.UnitPrice);
        }


        // Products with total quantity less than 10 across all locations
        var lowStockProducts = products
            .Select(product => new
            {
                product.Name,
                TotalQuantity = product.Inventories.Sum(i => i.Quantity)
            })
            .Where(product => product.TotalQuantity < 10)
            .OrderBy(product => product.TotalQuantity)
            .ToList();

        Console.WriteLine("=== Low stock products ===");
        foreach (var product in lowStockProducts)
        {
            Console.WriteLine($"Product: {product.Name}, Total Quantity: {product.TotalQuantity}");
        }


        // Warehouse with the most stock (total quantity of all products across all locations)
        var warehouseWithMostStock = warehouses
            .Select(warehouse => new
            {
                warehouse.Name,
                TotalStock = warehouse.Locations.Sum(location => location.Inventories.Sum(i => i.Quantity))
            })
            .OrderByDescending(warehouse => warehouse.TotalStock)
            .FirstOrDefault();

        Console.WriteLine();
        Console.WriteLine("=== Warehouse with most stock ===");
        if (warehouseWithMostStock != null)
        {
            Console.WriteLine($"Warehouse: {warehouseWithMostStock.Name}, Total Stock: {warehouseWithMostStock.TotalStock}");
        }
        else
        {
            Console.WriteLine("No warehouses found.");
        }

        // Delayed orders (orders that are pending or shipped but have an expected delivery date in the past)
        var delayedOrders = purchaseOrders
            .Select(order => new
            {
                order.OrderNumber,
                Supplier = order.Supplier.Name,
                Warehouse = order.Warehouse.Name,
                order.ExpectedDeliveryDate,
                order.Status
            })
            .Where(order =>
                (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Shipped) &&
                 order.ExpectedDeliveryDate < DateTime.Today)
            .OrderBy(order => order.ExpectedDeliveryDate)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("=== Delayed orders ===");
        foreach (var order in delayedOrders)
        {
            Console.WriteLine($"Order Number: {order.OrderNumber}, Supplier: {order.Supplier}, Warehouse: {order.Warehouse}, Expected Delivery: {order.ExpectedDeliveryDate.ToShortDateString()}, Status: {order.Status}");
        }

        // Supplier with with highest total value of orders (sum of quantity * unit price for all orders)
        var supplierWithHighestOrderValue = suppliers
            .Select(supplier => new
            {
                supplier.Name,
                TotalOrderValue = supplier.PurchaseOrders.Sum(order => order.PurchaseOrderItems.Sum(i => i.Quantity * i.UnitPrice))
            })
            .OrderByDescending(supplier => supplier.TotalOrderValue)
            .FirstOrDefault();

        Console.WriteLine();
        Console.WriteLine("=== Supplier with highest total order value ===");
        if (supplierWithHighestOrderValue != null)
        {
            Console.WriteLine($"Supplier: {supplierWithHighestOrderValue.Name}, Total Order Value: {supplierWithHighestOrderValue.TotalOrderValue:C}");
        }
        else
        {
            Console.WriteLine("No suppliers found.");
        }
        Console.WriteLine();

    }

}
