using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace WarehouseManagementSystem.Tests.E2E
{
    public class PurchaseOrderItemPlaywrightTests
    {
        private const string BaseUrl = "https://localhost:44377";
        private const string TestEmail = "dinostupar68@gmail.com";
        private const string TestPassword = "Dino1234!";

        [Fact]
        public async Task PurchaseOrderItemCrud_ShouldWorkThroughBrowser_InTenSteps()
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                SlowMo = 150
            });

            var page = await browser.NewPageAsync();

            const string purchaseOrderSearchTerm = "1001";
            const string productName = "Laptop";
            var createdQuantity = (DateTime.Now.Second + 30).ToString();
            var editedQuantity = (DateTime.Now.Second + 31).ToString();

            // 1. Open login page.
            await page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

            // 2. Log in as Admin or Operator.
            await page.Locator("input[name='Input.Email']").FillAsync(TestEmail);
            await page.Locator("input[name='Input.Password']").FillAsync(TestPassword);
            await page.Locator("#login-submit").ClickAsync();

            await page.WaitForURLAsync("**/Home");

            // 3. Open Purchase Order Items list page.
            await page.GetByRole(AriaRole.Link, new() { Name = "Purchase Order Items", Exact = true }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Order Items List" }))
                .ToBeVisibleAsync();

            // 4. Open Create Purchase Order Item form.
            await page.GetByRole(AriaRole.Link, new() { Name = "+ Create Purchase Order Item" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Create Purchase Order Item" }))
                .ToBeVisibleAsync();

            // 5. Select purchase order through custom AJAX autocomplete.
            var purchaseOrderAutocomplete = page.Locator("[data-autocomplete]").Nth(0);

            await purchaseOrderAutocomplete
                .Locator("[data-autocomplete-input]")
                .FillAsync(purchaseOrderSearchTerm);

            await page.WaitForTimeoutAsync(600);

            var firstPurchaseOrderOption = purchaseOrderAutocomplete
                .Locator(".wms-autocomplete-option", new() { HasText = "PO-" })
                .First;

            await Expect(firstPurchaseOrderOption).ToBeVisibleAsync();
            await firstPurchaseOrderOption.ClickAsync();

            var selectedPurchaseOrderNumber = await purchaseOrderAutocomplete
                .Locator("[data-autocomplete-input]")
                .InputValueAsync();

            // 6. Select product through custom AJAX autocomplete.
            var productAutocomplete = page.Locator("[data-autocomplete]").Nth(1);

            await productAutocomplete
                .Locator("[data-autocomplete-input]")
                .FillAsync(productName);

            await page.WaitForTimeoutAsync(600);

            var firstProductOption = productAutocomplete
                .Locator(".wms-autocomplete-option")
                .First;

            await Expect(firstProductOption).ToBeVisibleAsync();
            await firstProductOption.ClickAsync();

            // 7. Fill quantity and unit price, then create the item.
            await page.Locator("input[name='Quantity']").FillAsync(createdQuantity);
            await page.Locator("input[name='UnitPrice']").FillAsync("1500");

            await page.GetByRole(AriaRole.Button, new() { Name = "Create PO Item" }).ClickAsync();

            await page.WaitForURLAsync("**/purchase-order-items");

            // 8. Use AJAX search and capture the created POI code.
            await page.Locator("#purchaseOrderItem-search").FillAsync(productName);

            var createdRow = page.Locator("#purchaseOrderItem-table-body tr")
                .Filter(new() { HasText = productName })
                .Filter(new() { HasText = selectedPurchaseOrderNumber })
                .Filter(new() { HasText = $"{createdQuantity} Units" })
                .Last;

            await Expect(createdRow).ToBeVisibleAsync();

            var createdItemCode = await createdRow.Locator("td").First.InnerTextAsync();

            await createdRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Item Info {createdItemCode}" }))
                .ToBeVisibleAsync();

            await Expect(page.Locator("body")).ToContainTextAsync(productName);
            await Expect(page.Locator("body")).ToContainTextAsync(selectedPurchaseOrderNumber);
            await Expect(page.Locator("body")).ToContainTextAsync($"{createdQuantity} Units");

            // 9. Edit purchase order item and verify update.
            await page.GetByRole(AriaRole.Link, new() { Name = "Edit Purchase Order Item" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Edit PO Item {createdItemCode}" }))
                .ToBeVisibleAsync();

            await page.Locator("input[name='Quantity']").FillAsync(editedQuantity);
            await page.Locator("input[name='UnitPrice']").FillAsync("1750");

            await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

            await page.WaitForURLAsync("**/purchase-order-items");

            await page.Locator("#purchaseOrderItem-search").FillAsync(createdItemCode);

            var editedRow = page.Locator("#purchaseOrderItem-table-body tr")
                .Filter(new() { HasText = createdItemCode })
                .Filter(new() { HasText = $"{editedQuantity} Units" });

            await Expect(editedRow).ToBeVisibleAsync();

            await editedRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.Locator("body")).ToContainTextAsync(createdItemCode);
            await Expect(page.Locator("body")).ToContainTextAsync(productName);
            await Expect(page.Locator("body")).ToContainTextAsync($"{editedQuantity} Units");

            // 10. Delete purchase order item and verify it is no longer shown in search results.
            await page.GetByRole(AriaRole.Link, new() { Name = "Delete Purchase Order Item" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Delete PO Item {createdItemCode}" }))
                .ToBeVisibleAsync();

            await page.GetByRole(AriaRole.Button, new() { Name = "Yes, Delete PO Item" }).ClickAsync();

            await page.WaitForURLAsync("**/purchase-order-items");

            await page.Locator("#purchaseOrderItem-search").FillAsync(createdItemCode);

            var tableText = await page.Locator("#purchaseOrderItem-table-body").InnerTextAsync();
            Assert.DoesNotContain(createdItemCode, tableText);
        }
    }
}
