# Sitemap

## Routing model

- Konvencionalna default ruta je definirana u `Program.cs` kao:
  - `{controller=Home}/{action=Index}/{id?}`
- U aplikaciji su definirane eksplicitne custom (atributne) rute za sve glavne entitete.
- Default ruta i dalje može dohvatiti akcije po imenima kontrolera i akcija, koristeći konvencionalne putanje.

## Custom (atributne) URL-ovi

| URL | Controller | Action | View |
| --- | --- | --- | --- |
| `/` | `HomeController` | `Index` | `Views/Home/Index.cshtml` |
| `/privacy` | `HomeController` | `Privacy` | `Views/Home/Privacy.cshtml` |
| `/error` | `HomeController` | `Error` | `Views/Shared/Error.cshtml` |
| `/categories` | `CategoryController` | `Index` | `Views/Category/Index.cshtml` |
| `/categories/{id}` | `CategoryController` | `Details` | `Views/Category/Details.cshtml` |
| `/products` | `ProductController` | `Index` | `Views/Product/Index.cshtml` |
| `/products/{id}` | `ProductController` | `Details` | `Views/Product/Details.cshtml` |
| `/products/price-above/{minPrice}` | `ProductController` | `PriceAbove` | `Views/Product/PriceAbove.cshtml` |
| `/warehouses` | `WarehouseController` | `Index` | `Views/Warehouse/Index.cshtml` |
| `/warehouses/{id}` | `WarehouseController` | `Details` | `Views/Warehouse/Details.cshtml` |
| `/warehouses/city/{city}` | `WarehouseController` | `FindByCity` | `Views/Warehouse/FindByCity.cshtml` |
| `/warehouses/capacity-above/{minCapacity}` | `WarehouseController` | `CapacityAbove` | `Views/Warehouse/CapacityAbove.cshtml` |
| `/locations` | `LocationController` | `Index` | `Views/Location/Index.cshtml` |
| `/locations/{id}` | `LocationController` | `Details` | `Views/Location/Details.cshtml` |
| `/suppliers` | `SupplierController` | `Index` | `Views/Supplier/Index.cshtml` |
| `/suppliers/{id}` | `SupplierController` | `Details` | `Views/Supplier/Details.cshtml` |
| `/purchase-orders` | `PurchaseOrderController` | `Index` | `Views/PurchaseOrder/Index.cshtml` |
| `/purchase-orders/{id}` | `PurchaseOrderController` | `Details` | `Views/PurchaseOrder/Details.cshtml` |
| `/purchase-orders/status/{status}` | `PurchaseOrderController` | `FindByOrderStatus` | `Views/PurchaseOrder/Index.cshtml` |
| `/purchase-order-items` | `PurchaseOrderItemController` | `Index` | `Views/PurchaseOrderItem/Index.cshtml` |
| `/purchase-order-items/{id}` | `PurchaseOrderItemController` | `Details` | `Views/PurchaseOrderItem/Details.cshtml` |
| `/inventories` | `InventoryController` | `Index` | `Views/Inventory/Index.cshtml` |
| `/inventories/{id}` | `InventoryController` | `Details` | `Views/Inventory/Details.cshtml` |

## Default (konvencionalne) URL-ove

- Default ruta koristi kontroler ime bez sufixa `Controller` i akciju.
- Za `HomeController` je default URL `/` zbog `controller=Home` i `action=Index`.
- Ako se koristi default route, putanje koriste kontrolerne nazive u jednini, npr. `/Product`, `/Category`, `/Warehouse`.

| Default URL | Controller | Action | View |
| --- | --- | --- | --- |
| `/` | `HomeController` | `Index` | `Views/Home/Index.cshtml` |
| `/Home` | `HomeController` | `Index` | `Views/Home/Index.cshtml` |
| `/Home/Index` | `HomeController` | `Index` | `Views/Home/Index.cshtml` |
| `/Home/Privacy` | `HomeController` | `Privacy` | `Views/Home/Privacy.cshtml` |
| `/Home/Error` | `HomeController` | `Error` | `Views/Shared/Error.cshtml` |
| `/Category` | `CategoryController` | `Index` | `Views/Category/Index.cshtml` |
| `/Category/Details/{id}` | `CategoryController` | `Details` | `Views/Category/Details.cshtml` |
| `/Product` | `ProductController` | `Index` | `Views/Product/Index.cshtml` |
| `/Product/Details/{id}` | `ProductController` | `Details` | `Views/Product/Details.cshtml` |
| `/Warehouse` | `WarehouseController` | `Index` | `Views/Warehouse/Index.cshtml` |
| `/Warehouse/Details/{id}` | `WarehouseController` | `Details` | `Views/Warehouse/Details.cshtml` |
| `/Warehouse/FindByCity/{city}` | `WarehouseController` | `FindByCity` | `Views/Warehouse/FindByCity.cshtml` |
| `/Warehouse/CapacityAbove/{minCapacity}` | `WarehouseController` | `CapacityAbove` | `Views/Warehouse/CapacityAbove.cshtml` |
| `/Location` | `LocationController` | `Index` | `Views/Location/Index.cshtml` |
| `/Location/Details/{id}` | `LocationController` | `Details` | `Views/Location/Details.cshtml` |
| `/Supplier` | `SupplierController` | `Index` | `Views/Supplier/Index.cshtml` |
| `/Supplier/Details/{id}` | `SupplierController` | `Details` | `Views/Supplier/Details.cshtml` |
| `/PurchaseOrder` | `PurchaseOrderController` | `Index` | `Views/PurchaseOrder/Index.cshtml` |
| `/PurchaseOrder/Details/{id}` | `PurchaseOrderController` | `Details` | `Views/PurchaseOrder/Details.cshtml` |
| `/PurchaseOrder/FindByOrderStatus/{status}` | `PurchaseOrderController` | `FindByOrderStatus` | `Views/PurchaseOrder/Index.cshtml` |
| `/PurchaseOrderItem` | `PurchaseOrderItemController` | `Index` | `Views/PurchaseOrderItem/Index.cshtml` |
| `/PurchaseOrderItem/Details/{id}` | `PurchaseOrderItemController` | `Details` | `Views/PurchaseOrderItem/Details.cshtml` |
| `/Inventory` | `InventoryController` | `Index` | `Views/Inventory/Index.cshtml` |
| `/Inventory/Details/{id}` | `InventoryController` | `Details` | `Views/Inventory/Details.cshtml` |

## Napomene

- Sve Views stranice koriste zajednički layout `Views/Shared/_Layout.cshtml`.
- Viewovi također mogu uključivati pomoćne dijelove poput `Views/Shared/_ValidationScriptsPartial.cshtml`.
- Custom routes su glavni URL-ovi, a default ruta je fallback koja koristi nazive kontrolera i akcija.
