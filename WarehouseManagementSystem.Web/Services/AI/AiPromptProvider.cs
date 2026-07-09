namespace WarehouseManagementSystem.Web.Services.AI
{
    public class AiPromptProvider
    {
        public string GetSystemPrompt(string entity)
        {
            return entity.ToLower() switch
            {
                "category" => CategoryPrompt,
                "warehouse" => WarehousePrompt,
                "supplier" => SupplierPrompt,
                "product" => ProductPrompt,
                "location" => LocationPrompt,
                "inventory" => InventoryPrompt,
                "purchaseorder" => PurchaseOrderPrompt,
                "purchaseorderitem" => PurchaseOrderItemPrompt,
                _ => throw new ArgumentException("Unsupported AI entity.")
            };
        }

        private const string BaseRules = """
        Return only valid JSON.
        Do not include markdown.
        Do not include explanations.
        If a field is missing, return null.
        Dates must be returned in ISO format: yyyy-MM-ddTHH:mm.
        """;

        private static readonly string CategoryPrompt = BaseRules + """

        Convert user text into this JSON:
        {
          "name": string,
          "description": string
        }
        """;

        private static readonly string WarehousePrompt = BaseRules + """

        Convert user text into this JSON:
        {
          "name": string,
          "address": string,
          "city": string,
          "country": string,
          "capacity": number
        }
        """;

        private static readonly string SupplierPrompt = BaseRules + """

        Convert user text into this JSON:
        {
          "name": string,
          "contactPerson": string,
          "contactEmail": string,
          "contactPhone": string,
          "contactAddress": string
        }
        """;

        private static readonly string ProductPrompt = BaseRules + """

        Convert user text into this JSON:
        {
          "name": string,
          "description": string,
          "categoryName": string,
          "price": number,
          "weight": number,
          "productReceivedAt": string
        }

        Price is in euros.
        Weight is in kilograms.
        """;

        private static readonly string LocationPrompt = BaseRules + """

        Convert user text into this JSON:
        {
          "code": string,
          "zone": string,
          "shelfNumber": number,
          "warehouseName": string
        }
        """;

        private static readonly string InventoryPrompt = BaseRules + """

        Convert user text into this JSON:
        {
          "productName": string,
          "locationCode": string,
          "quantity": number,
          "lastUpdated": string
        }
        """;

        private static readonly string PurchaseOrderPrompt = BaseRules + """

        Convert user text into this JSON:
        {
          "supplierName": string,
          "warehouseName": string,
          "totalAmount": number,
          "status": string,
          "orderDate": string,
          "expectedDeliveryDate": string
        }

        Total amount is in euros.
        Status should be one of: Pending, Approved, Shipped, Delivered, Cancelled.
        """;

        private static readonly string PurchaseOrderItemPrompt = BaseRules + """

        Convert user text into this JSON:
        {
          "purchaseOrderNumber": string,
          "productName": string,
          "quantity": number,
          "unitPrice": number
        }

        Purchase order number should be returned without the PO prefix when possible, for example "1001".
        Unit price is in euros.
        """;

    }
}
