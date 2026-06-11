using WarehouseManagementSystem.Model;
using WarehouseManagementSystem.Web.Dtos;

namespace WarehouseManagementSystem.Web.Dtos
{
    public static class ApiMapper
    {
        public static CategoryDto ToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public static Category ToEntity(CategoryCreateDto dto)
        {
            return new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };
        }

        public static void UpdateEntity(Category category, CategoryUpdateDto dto)
        {
            category.Name = dto.Name;
            category.Description = dto.Description;
        }

        public static WarehouseDto ToDto(Warehouse warehouse)
        {
            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address,
                City = warehouse.City,
                Country = warehouse.Country,
                Capacity = warehouse.Capacity,
                Locations = warehouse.Locations?
                    .Select(ToSummaryDto)
                    .ToList() ?? new List<LocationSummaryDto>()
            };
        }

        public static LocationSummaryDto ToSummaryDto(Location location)
        {
            return new LocationSummaryDto
            {
                Id = location.Id,
                Code = location.Code,
                Zone = location.Zone,
                ShelfNumber = location.ShelfNumber
            };
        }

        public static Warehouse ToEntity(WarehouseCreateDto dto)
        {
            return new Warehouse
            {
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                Capacity = dto.Capacity
            };
        }

        public static void UpdateEntity(Warehouse warehouse, WarehouseUpdateDto dto)
        {
            warehouse.Name = dto.Name;
            warehouse.Address = dto.Address;
            warehouse.City = dto.City;
            warehouse.Country = dto.Country;
            warehouse.Capacity = dto.Capacity;
        }

        public static SupplierDto ToDto(Supplier supplier)
        {
            return new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                ContactEmail = supplier.ContactEmail,
                ContactPhone = supplier.ContactPhone,
                ContactAddress = supplier.ContactAddress,
                PurchaseOrderCount = supplier.PurchaseOrders?.Count ?? 0
            };
        }

        public static Supplier ToEntity(SupplierCreateDto dto)
        {
            return new Supplier
            {
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                ContactAddress = dto.ContactAddress
            };
        }

        public static void UpdateEntity(Supplier supplier, SupplierUpdateDto dto)
        {
            supplier.Name = dto.Name;
            supplier.ContactPerson = dto.ContactPerson;
            supplier.ContactEmail = dto.ContactEmail;
            supplier.ContactPhone = dto.ContactPhone;
            supplier.ContactAddress = dto.ContactAddress;
        }

        public static ProductDto ToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Weight = product.Weight,
                ProductReceivedAt = product.ProductReceivedAt,
                Category = product.Category == null ? null : ToDto(product.Category)
            };
        }

        public static Product ToEntity(ProductCreateDto dto)
        {
            return new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Weight = dto.Weight,
                ProductReceivedAt = dto.ProductReceivedAt,
                CategoryId = dto.CategoryId
            };
        }

        public static void UpdateEntity(Product product, ProductUpdateDto dto)
        {
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Weight = dto.Weight;
            product.ProductReceivedAt = dto.ProductReceivedAt;
            product.CategoryId = dto.CategoryId;
        }

        public static LocationDto ToDto(Location location)
        {
            return new LocationDto
            {
                Id = location.Id,
                Code = location.Code,
                Zone = location.Zone,
                ShelfNumber = location.ShelfNumber,
                Warehouse = location.Warehouse == null ? null : ToSummaryDto(location.Warehouse)
            };
        }

        public static WarehouseSummaryDto ToSummaryDto(Warehouse warehouse)
        {
            return new WarehouseSummaryDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                City = warehouse.City
            };
        }

        public static Location ToEntity(LocationCreateDto dto)
        {
            return new Location
            {
                Code = dto.Code,
                Zone = dto.Zone,
                ShelfNumber = dto.ShelfNumber,
                WarehouseId = dto.WarehouseId
            };
        }

        public static void UpdateEntity(Location location, LocationUpdateDto dto)
        {
            location.Code = dto.Code;
            location.Zone = dto.Zone;
            location.ShelfNumber = dto.ShelfNumber;
            location.WarehouseId = dto.WarehouseId;
        }

        public static InventoryDto ToDto(Inventory inventory)
        {
            return new InventoryDto
            {
                Id = inventory.Id,
                Quantity = inventory.Quantity,
                LastUpdated = inventory.LastUpdated,
                Product = inventory.Product == null ? null : ToSummaryDto(inventory.Product),
                Location = inventory.Location == null ? null : ToSummaryDto(inventory.Location)
            };
        }

        public static ProductSummaryDto ToSummaryDto(Product product)
        {
            return new ProductSummaryDto
            {
                Id = product.Id,
                Name = product.Name
            };
        }

        public static Inventory ToEntity(InventoryCreateDto dto)
        {
            return new Inventory
            {
                Quantity = dto.Quantity,
                LastUpdated = dto.LastUpdated,
                ProductId = dto.ProductId,
                LocationId = dto.LocationId
            };
        }

        public static void UpdateEntity(Inventory inventory, InventoryUpdateDto dto)
        {
            inventory.Quantity = dto.Quantity;
            inventory.LastUpdated = dto.LastUpdated;
            inventory.ProductId = dto.ProductId;
            inventory.LocationId = dto.LocationId;
        }

        public static PurchaseOrderDto ToDto(PurchaseOrder purchaseOrder)
        {
            return new PurchaseOrderDto
            {
                Id = purchaseOrder.Id,
                OrderNumber = purchaseOrder.OrderNumber,
                OrderDate = purchaseOrder.OrderDate,
                ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
                TotalAmount = purchaseOrder.TotalAmount,
                Status = purchaseOrder.Status,
                Supplier = purchaseOrder.Supplier == null ? null : ToSummaryDto(purchaseOrder.Supplier),
                Warehouse = purchaseOrder.Warehouse == null ? null : ToSummaryDto(purchaseOrder.Warehouse)
            };
        }

        public static SupplierSummaryDto ToSummaryDto(Supplier supplier)
        {
            return new SupplierSummaryDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactEmail = supplier.ContactEmail
            };
        }

        public static PurchaseOrder ToEntity(PurchaseOrderCreateDto dto, int orderNumber)
        {
            return new PurchaseOrder
            {
                OrderNumber = orderNumber,
                OrderDate = dto.OrderDate,
                ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                TotalAmount = dto.TotalAmount,
                Status = dto.Status,
                SupplierId = dto.SupplierId,
                WarehouseId = dto.WarehouseId
            };
        }

        public static void UpdateEntity(PurchaseOrder purchaseOrder, PurchaseOrderUpdateDto dto)
        {
            purchaseOrder.OrderDate = dto.OrderDate;
            purchaseOrder.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
            purchaseOrder.TotalAmount = dto.TotalAmount;
            purchaseOrder.Status = dto.Status;
            purchaseOrder.SupplierId = dto.SupplierId;
            purchaseOrder.WarehouseId = dto.WarehouseId;
        }

        public static PurchaseOrderItemDto ToDto(PurchaseOrderItem item)
        {
            return new PurchaseOrderItemDto
            {
                Id = item.Id,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                PurchaseOrder = item.PurchaseOrder == null ? null : ToSummaryDto(item.PurchaseOrder),
                Product = item.Product == null ? null : ToSummaryDto(item.Product)
            };
        }

        public static PurchaseOrderSummaryDto ToSummaryDto(PurchaseOrder purchaseOrder)
        {
            return new PurchaseOrderSummaryDto
            {
                Id = purchaseOrder.Id,
                OrderNumber = purchaseOrder.OrderNumber,
                OrderDate = purchaseOrder.OrderDate
            };
        }

        public static PurchaseOrderItem ToEntity(PurchaseOrderItemCreateDto dto)
        {
            return new PurchaseOrderItem
            {
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                PurchaseOrderId = dto.PurchaseOrderId,
                ProductId = dto.ProductId
            };
        }

        public static void UpdateEntity(PurchaseOrderItem item, PurchaseOrderItemUpdateDto dto)
        {
            item.Quantity = dto.Quantity;
            item.UnitPrice = dto.UnitPrice;
            item.PurchaseOrderId = dto.PurchaseOrderId;
            item.ProductId = dto.ProductId;
        }

    }
}
