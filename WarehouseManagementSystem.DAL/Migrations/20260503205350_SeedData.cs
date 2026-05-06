using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WarehouseManagementSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Electronic devices and accessories", "Electronics" },
                    { 2, "Warehouse furniture and storage solutions", "Furniture" },
                    { 3, "General office supplies and equipment", "Office Supplies" }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "ContactAddress", "ContactEmail", "ContactPerson", "ContactPhone", "Name" },
                values: new object[,]
                {
                    { 1, "910 Industrial Park Road, Chicago, USA", "michael.carter@autoidsystems.com", "Michael Carter", "+1-312-555-0142", "AutoID Systems" },
                    { 2, "455 Technology Avenue, Columbus, USA", "laura.bennett@techcore.com", "Laura Bennett", "+1-614-555-0187", "TechCore Solutions" },
                    { 3, "220 Business Center Drive, Phoenix, USA", "daniel.foster@officefurnishings.com", "Daniel Foster", "+1-602-555-0119", "Office Furnishings Group" }
                });

            migrationBuilder.InsertData(
                table: "Warehouses",
                columns: new[] { "Id", "Address", "Capacity", "City", "Country", "Name" },
                values: new object[,]
                {
                    { 1, "1250 Logistics Parkway", 1000, "Chicago", "USA", "Main Distribution Center" },
                    { 2, "840 Industrial Avenue", 750, "Columbus", "USA", "Eastern Fulfillment Hub" },
                    { 3, "620 Commerce Drive", 500, "Phoenix", "USA", "Western Logistics Center" }
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "Code", "ShelfNumber", "WarehouseId", "Zone" },
                values: new object[,]
                {
                    { 1, "MDC-A-01", 1, 1, "A" },
                    { 2, "MDC-B-03", 3, 1, "B" },
                    { 3, "EFH-A-01", 1, 2, "A" },
                    { 4, "EFH-C-06", 6, 2, "C" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "ProductReceivedAt", "Weight" },
                values: new object[,]
                {
                    { 1, 3, "Portable barcode scanner designed for fast and accurate inventory processing", "Handheld Barcode Scanner", 120.00m, new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), 0.35m },
                    { 2, 3, "High-speed thermal printer for warehouse and shipping labels", "Industrial Label Printer", 230m, new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2.10m },
                    { 3, 1, "15-inch laptop built for office productivity and warehouse administration", "Laptop", 1500m, new DateTime(2026, 2, 18, 0, 0, 0, 0, DateTimeKind.Utc), 1.5m },
                    { 4, 1, "Full HD monitor suitable for administrative and operational workstations", "Monitor", 300m, new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), 3.0m },
                    { 5, 2, "Adjustable office chair designed for long-duration seated work", "Ergonomic Office Chair", 250m, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), 15m },
                    { 6, 2, "Durable desk with ample surface area for office and warehouse coordination tasks", "Workstation Desk", 400m, new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Utc), 30m }
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrders",
                columns: new[] { "Id", "ExpectedDeliveryDate", "OrderDate", "OrderNumber", "Status", "SupplierId", "TotalAmount", "WarehouseId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1001, 2, 1, 29000m, 1 },
                    { 2, new DateTime(2026, 3, 16, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 8, 0, 0, 0, 0, DateTimeKind.Utc), 1002, 2, 2, 51000m, 2 },
                    { 3, new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 12, 0, 0, 0, 0, DateTimeKind.Utc), 1003, 3, 3, 20000m, 3 }
                });

            migrationBuilder.InsertData(
                table: "Inventories",
                columns: new[] { "Id", "LastUpdated", "LocationId", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 30, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 1 },
                    { 2, new DateTime(2026, 3, 27, 0, 0, 0, 0, DateTimeKind.Utc), 2, 2, 5 },
                    { 3, new DateTime(2026, 3, 23, 0, 0, 0, 0, DateTimeKind.Utc), 3, 3, 25 },
                    { 4, new DateTime(2026, 3, 25, 0, 0, 0, 0, DateTimeKind.Utc), 4, 4, 100 },
                    { 5, new DateTime(2026, 3, 22, 0, 0, 0, 0, DateTimeKind.Utc), 3, 5, 255 },
                    { 6, new DateTime(2026, 3, 26, 0, 0, 0, 0, DateTimeKind.Utc), 3, 6, 500 }
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrderItems",
                columns: new[] { "Id", "ProductId", "PurchaseOrderId", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 1, 1, 50, 120m },
                    { 2, 2, 1, 100, 230m },
                    { 3, 3, 2, 30, 1500m },
                    { 4, 4, 2, 20, 300m },
                    { 5, 5, 3, 40, 250m },
                    { 6, 6, 3, 25, 400m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderItems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderItems",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PurchaseOrderItems",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PurchaseOrders",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
