# Semantički DB model

## Pregled entiteta

Ovaj projekt koristi sljedeće glavne domenske modele (entitete/tablice):

- `Warehouse`
- `Location`
- `Inventory`
- `Category`
- `Product`
- `Supplier`
- `PurchaseOrder`
- `PurchaseOrderItem`
- `OrderStatus` (enumeracija)

## Entiteti i glavna svojstva

### Warehouse
- `Id` (PK)
- `Name`
- `Address`
- `City`
- `Country`
- `Capacity`
- `Locations` (1:N veza prema `Location`)
- `PurchaseOrders` (1:N veza prema `PurchaseOrder`)

### Location
- `Id` (PK)
- `Code`
- `Zone`
- `ShelfNumber`
- `WarehouseId` (FK)
- `Warehouse` (navigacijsko svojstvo)
- `Inventories` (1:N veza prema `Inventory`)

### Inventory
- `Id` (PK)
- `Quantity`
- `LastUpdated`
- `ProductId` (FK)
- `Product` (navigacijsko svojstvo)
- `LocationId` (FK)
- `Location` (navigacijsko svojstvo)

### Category
- `Id` (PK)
- `Name`
- `Description`
- `Products` (1:N veza prema `Product`)

### Product
- `Id` (PK)
- `Name`
- `Description`
- `Price`
- `Weight`
- `ProductReceivedAt`
- `CategoryId` (FK)
- `Category` (navigacijsko svojstvo)
- `Inventories` (1:N veza prema `Inventory`)
- `PurchaseOrderItems` (1:N veza prema `PurchaseOrderItem`)

### Supplier
- `Id` (PK)
- `Name`
- `ContactPerson`
- `ContactEmail`
- `ContactPhone`
- `ContactAddress`
- `PurchaseOrders` (1:N veza prema `PurchaseOrder`)

### PurchaseOrder
- `Id` (PK)
- `OrderNumber`
- `OrderDate`
- `ExpectedDeliveryDate`
- `TotalAmount`
- `Status` (`OrderStatus` enum)
- `SupplierId` (FK)
- `Supplier` (navigacijsko svojstvo)
- `WarehouseId` (FK)
- `Warehouse` (navigacijsko svojstvo)
- `PurchaseOrderItems` (1:N veza prema `PurchaseOrderItem`)

### PurchaseOrderItem
- `Id` (PK)
- `Quantity`
- `UnitPrice`
- `PurchaseOrderId` (FK)
- `PurchaseOrder` (navigacijsko svojstvo)
- `ProductId` (FK)
- `Product` (navigacijsko svojstvo)

### OrderStatus
Enumeracija stanja narudžbe:
- `Pending`
- `Approved`
- `Shipped`
- `Delivered`
- `Cancelled`

## Glavne veze među entitetima

- `Warehouse` 1:N `Location`
- `Warehouse` 1:N `PurchaseOrder`
- `Location` 1:N `Inventory`
- `Product` 1:N `Inventory`
- `Product` 1:N `PurchaseOrderItem`
- `Category` 1:N `Product`
- `Supplier` 1:N `PurchaseOrder`
- `PurchaseOrder` 1:N `PurchaseOrderItem`

## DbContext tablice

U `WarehouseManagementSystemDbContext` su definirani svi entiteti kao `DbSet<T>`:
- `Warehouses`
- `Locations`
- `Inventories`
- `Products`
- `Categories`
- `Suppliers`
- `PurchaseOrders`
- `PurchaseOrderItems`

## Napomene

- Veze su već konfigurirane u `OnModelCreating` uz eksplicitne `HasForeignKey` i `DeleteBehavior` opcije.
- Model sadrži početne podatke (`HasData`) za skladišta, lokacije, kategorije, proizvode, inventar, dobavljače i narudžbe.
