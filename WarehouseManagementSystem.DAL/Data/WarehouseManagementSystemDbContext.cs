using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Model;

namespace WarehouseManagementSystem.DAL.Data
{
    public class WarehouseManagementSystemDbContext : DbContext
    {
        public WarehouseManagementSystemDbContext(DbContextOptions<WarehouseManagementSystemDbContext> options) 
            : base(options)
        {}

        public override int SaveChanges()
        {
            NormalizeDateTimesForPostgres();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            NormalizeDateTimesForPostgres();
            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<Warehouse> Warehouses { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<Inventory> Inventories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

        private void NormalizeDateTimesForPostgres()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                foreach (var property in entry.Properties
                    .Where(p => p.Metadata.ClrType == typeof(DateTime)))
                {
                    if (property.CurrentValue is not DateTime value)
                    {
                        continue;
                    }

                    property.CurrentValue = value.Kind switch
                    {
                        DateTimeKind.Utc => value,
                        DateTimeKind.Local => value.ToUniversalTime(),
                        _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
                    };
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Warehouse>()
                .HasMany(w => w.Locations)
                .WithOne(l => l.Warehouse)
                .HasForeignKey(l => l.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Warehouse>()
                .HasMany(w => w.PurchaseOrders)
                .WithOne(po => po.Warehouse)
                .HasForeignKey(po => po.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Location>()
                .HasMany(l => l.Inventories)
                .WithOne(i => i.Location)
                .HasForeignKey(i => i.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Inventories)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.PurchaseOrderItems)
                .WithOne(poi => poi.Product)
                .HasForeignKey(poi => poi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.PurchaseOrders)
                .WithOne(po => po.Supplier)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(po => po.PurchaseOrderItems)
                .WithOne(poi => poi.PurchaseOrder)
                .HasForeignKey(poi => poi.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = 1, Name = "Main Distribution Center", Address = "1250 Logistics Parkway", City = "Chicago", Country = "USA", Capacity = 1000 },
                new Warehouse { Id = 2, Name = "Eastern Fulfillment Hub", Address = "840 Industrial Avenue", City = "Columbus", Country = "USA", Capacity = 750 },
                new Warehouse { Id = 3, Name = "Western Logistics Center", Address = "620 Commerce Drive", City = "Phoenix", Country = "USA", Capacity = 500 }
            );

            modelBuilder.Entity<Location>().HasData(
                new Location { Id = 1, Code = "MDC-A-01", Zone = "A", ShelfNumber = 1, WarehouseId = 1 },
                new Location { Id = 2, Code = "MDC-B-03", Zone = "B", ShelfNumber = 3, WarehouseId = 1 },
                new Location { Id = 3, Code = "EFH-A-01", Zone = "A", ShelfNumber = 1, WarehouseId = 2 },
                new Location { Id = 4, Code = "EFH-C-06", Zone = "C", ShelfNumber = 6, WarehouseId = 2 }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
                new Category { Id = 2, Name = "Furniture", Description = "Warehouse furniture and storage solutions" },
                new Category { Id = 3, Name = "Office Supplies", Description = "General office supplies and equipment" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Handheld Barcode Scanner", Description = "Portable barcode scanner designed for fast and accurate inventory processing", Price = 120.00m, Weight = 0.35m, ProductReceivedAt = new DateTime(2026, 01, 06, 0, 0, 0, DateTimeKind.Utc), CategoryId = 3 },
                new Product { Id = 2, Name = "Industrial Label Printer", Description = "High-speed thermal printer for warehouse and shipping labels", Price = 230m, Weight = 2.10m, ProductReceivedAt = new DateTime(2026, 02, 15, 0, 0, 0, DateTimeKind.Utc), CategoryId = 3 },
                new Product { Id = 3, Name = "Laptop", Description = "15-inch laptop built for office productivity and warehouse administration", Price = 1500m, Weight = 1.5m, ProductReceivedAt = new DateTime(2026, 02, 18, 0, 0, 0, DateTimeKind.Utc), CategoryId = 1 },
                new Product { Id = 4, Name = "Monitor", Description = "Full HD monitor suitable for administrative and operational workstations", Price = 300m, Weight = 3.0m, ProductReceivedAt = new DateTime(2026, 03, 07, 0, 0, 0, DateTimeKind.Utc), CategoryId = 1 },
                new Product { Id = 5, Name = "Ergonomic Office Chair", Description = "Adjustable office chair designed for long-duration seated work", Price = 250m, Weight = 15m, ProductReceivedAt = new DateTime(2026, 03, 18, 0, 0, 0, DateTimeKind.Utc), CategoryId = 2 },
                new Product { Id = 6, Name = "Workstation Desk", Description = "Durable desk with ample surface area for office and warehouse coordination tasks", Price = 400m, Weight = 30m, ProductReceivedAt = new DateTime(2026, 03, 13, 0, 0, 0, DateTimeKind.Utc), CategoryId = 2 }
            );

            modelBuilder.Entity<Inventory>().HasData(
                new Inventory { Id = 1, ProductId = 1, LocationId = 1, Quantity = 1, LastUpdated = new DateTime(2026, 03, 30, 0, 0, 0, DateTimeKind.Utc) },
                new Inventory { Id = 2, ProductId = 2, LocationId = 2, Quantity = 5, LastUpdated = new DateTime(2026, 03, 27, 0, 0, 0, DateTimeKind.Utc) },
                new Inventory { Id = 3, ProductId = 3, LocationId = 3, Quantity = 25, LastUpdated = new DateTime(2026, 03, 23, 0, 0, 0, DateTimeKind.Utc) },
                new Inventory { Id = 4, ProductId = 4, LocationId = 4, Quantity = 100, LastUpdated = new DateTime(2026, 03, 25, 0, 0, 0, DateTimeKind.Utc) },
                new Inventory { Id = 5, ProductId = 5, LocationId = 3, Quantity = 255, LastUpdated = new DateTime(2026, 03, 22, 0, 0, 0, DateTimeKind.Utc) },
                new Inventory { Id = 6, ProductId = 6, LocationId = 3, Quantity = 500, LastUpdated = new DateTime(2026, 03, 26, 0, 0, 0, DateTimeKind.Utc) }
            );

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = 1, Name = "AutoID Systems", ContactPerson = "Michael Carter", ContactEmail = "michael.carter@autoidsystems.com", ContactPhone = "+1-312-555-0142", ContactAddress = "910 Industrial Park Road, Chicago, USA" },
                new Supplier { Id = 2, Name = "TechCore Solutions", ContactPerson = "Laura Bennett", ContactEmail = "laura.bennett@techcore.com", ContactPhone = "+1-614-555-0187", ContactAddress = "455 Technology Avenue, Columbus, USA" },
                new Supplier { Id = 3, Name = "Office Furnishings Group", ContactPerson = "Daniel Foster", ContactEmail = "daniel.foster@officefurnishings.com", ContactPhone = "+1-602-555-0119", ContactAddress = "220 Business Center Drive, Phoenix, USA" }
            );

            modelBuilder.Entity<PurchaseOrder>().HasData(
                new PurchaseOrder { Id = 1, OrderNumber = 1001, OrderDate = new DateTime(2026, 03, 02, 0, 0, 0, DateTimeKind.Utc), ExpectedDeliveryDate = new DateTime(2026, 03, 09, 0, 0, 0, DateTimeKind.Utc), Status = OrderStatus.Shipped, SupplierId = 1, WarehouseId = 1, TotalAmount = 29000m },
                new PurchaseOrder { Id = 2, OrderNumber = 1002, OrderDate = new DateTime(2026, 03, 08, 0, 0, 0, DateTimeKind.Utc), ExpectedDeliveryDate = new DateTime(2026, 03, 16, 0, 0, 0, DateTimeKind.Utc), Status = OrderStatus.Shipped, SupplierId = 2, WarehouseId = 2, TotalAmount = 51000m },
                new PurchaseOrder { Id = 3, OrderNumber = 1003, OrderDate = new DateTime(2026, 03, 12, 0, 0, 0, DateTimeKind.Utc), ExpectedDeliveryDate = new DateTime(2026, 03, 21, 0, 0, 0, DateTimeKind.Utc), Status = OrderStatus.Delivered, SupplierId = 3, WarehouseId = 3, TotalAmount = 20000m }
            );

            modelBuilder.Entity<PurchaseOrderItem>().HasData(
                new PurchaseOrderItem { Id = 1, PurchaseOrderId = 1, ProductId = 1, Quantity = 50, UnitPrice = 120m },
                new PurchaseOrderItem { Id = 2, PurchaseOrderId = 1, ProductId = 2, Quantity = 100, UnitPrice = 230m },
                new PurchaseOrderItem { Id = 3, PurchaseOrderId = 2, ProductId = 3, Quantity = 30, UnitPrice = 1500m },
                new PurchaseOrderItem { Id = 4, PurchaseOrderId = 2, ProductId = 4, Quantity = 20, UnitPrice = 300m },
                new PurchaseOrderItem { Id = 5, PurchaseOrderId = 3, ProductId = 5, Quantity = 40, UnitPrice = 250m },
                new PurchaseOrderItem { Id = 6, PurchaseOrderId = 3, ProductId = 6, Quantity = 25, UnitPrice = 400m }
            );
        }
    }
}
