using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace WarehouseManagementSystem.Tests.E2E
{
    public class PurchaseOrderPlaywrightTests
    {
        private const string BaseUrl = "https://localhost:44377";
        private const string TestEmail = "dinostupar68@gmail.com";
        private const string TestPassword = "Dino1234!";

        [Fact]
        public async Task PurchaseOrderCrud_ShouldWorkThroughBrowser_InTenSteps()
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                SlowMo = 150
            });

            var page = await browser.NewPageAsync();

            // 1. Open login page.
            await page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

            // 2. Log in as Admin or Operator.
            await page.Locator("input[name='Input.Email']").FillAsync(TestEmail);
            await page.Locator("input[name='Input.Password']").FillAsync(TestPassword);
            await page.Locator("#login-submit").ClickAsync();

            await page.WaitForURLAsync("**/Home");

            // 3. Open Purchase Orders list page.
            await page.GetByRole(AriaRole.Link, new() { Name = "Purchase Orders", Exact = true }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Purchase Orders" }))
                .ToBeVisibleAsync();

            // 4. Open Create Purchase Order form.
            await page.GetByRole(AriaRole.Link, new() { Name = "+ Create Purchase Order" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Create Purchase Order" }))
                .ToBeVisibleAsync();

            // 5. Fill status, dates and total amount.
            await page.Locator("select[name='Status']").SelectOptionAsync(new SelectOptionValue
            {
                Label = "Approved"
            });

            var dateTimeInputs = page.Locator("[data-datetime-display]");

            await dateTimeInputs.Nth(0).FillAsync("01.07.2026. 10:00");
            await dateTimeInputs.Nth(1).FillAsync("05.07.2026. 09:00");

            await page.Locator("input[name='TotalAmount']").FillAsync("1250.75");

            // 6. Select supplier through custom AJAX autocomplete.
            var supplierAutocomplete = page.Locator("[data-autocomplete]").Nth(0);

            await supplierAutocomplete
                .Locator("[data-autocomplete-input]")
                .FillAsync("AutoID Systems");

            var firstSupplierOption = supplierAutocomplete
                .Locator(".wms-autocomplete-option")
                .First;

            await Expect(firstSupplierOption).ToBeVisibleAsync();
            await firstSupplierOption.ClickAsync();

            // 7. Select warehouse through custom AJAX autocomplete.
            var warehouseAutocomplete = page.Locator("[data-autocomplete]").Nth(1);

            await warehouseAutocomplete
                .Locator("[data-autocomplete-input]")
                .FillAsync("Main Distribution Center");

            var firstWarehouseOption = warehouseAutocomplete
                .Locator(".wms-autocomplete-option")
                .First;

            await Expect(firstWarehouseOption).ToBeVisibleAsync();
            await firstWarehouseOption.ClickAsync();

            // 8. Submit create form and verify with AJAX search.
            await page.GetByRole(AriaRole.Button, new() { Name = "Create Purchase Order" }).ClickAsync();

            await page.WaitForURLAsync("**/purchase-orders");

            await page.Locator("#purchaseOrder-search").FillAsync("AutoID Systems");

            var createdRow = page.Locator("#purchaseOrder-table-body tr")
                .Filter(new() { HasText = "AutoID Systems" })
                .Filter(new() { HasText = "Approved" })
                .Last;

            await Expect(createdRow).ToBeVisibleAsync();

            var createdOrderNumber = await createdRow.Locator("td").First.InnerTextAsync();

            await createdRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { NameRegex = new("Order Info PO-") }))
                .ToBeVisibleAsync();

            await Expect(page.Locator("body")).ToContainTextAsync("AutoID Systems");
            await Expect(page.Locator("body")).ToContainTextAsync("Main Distribution Center");
            await Expect(page.Locator("body")).ToContainTextAsync("Approved");

            // 9. Edit purchase order and verify update.
            await page.GetByRole(AriaRole.Link, new() { Name = "Edit Purchase Order" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { NameRegex = new("Edit Purchase Order PO-") }))
                .ToBeVisibleAsync();

            await page.Locator("select[name='Status']").SelectOptionAsync(new SelectOptionValue
            {
                Label = "Shipped"
            });

            await page.Locator("input[name='TotalAmount']").FillAsync("1499.99");

            dateTimeInputs = page.Locator("[data-datetime-display]");

            await dateTimeInputs.Nth(0).FillAsync("02.07.2026. 11:30");
            await dateTimeInputs.Nth(1).FillAsync("06.07.2026. 12:00");

            await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

            await page.WaitForURLAsync("**/purchase-orders");

            await page.Locator("#purchaseOrder-search").FillAsync("AutoID Systems");

            var editedRow = page.Locator("#purchaseOrder-table-body tr")
                .Filter(new() { HasText = createdOrderNumber })
                .Filter(new() { HasText = "Shipped" });

            await Expect(editedRow).ToBeVisibleAsync();

            await editedRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.Locator("body")).ToContainTextAsync("AutoID Systems");
            await Expect(page.Locator("body")).ToContainTextAsync("Main Distribution Center");
            await Expect(page.Locator("body")).ToContainTextAsync("Shipped");

            // 10. Delete purchase order and verify it is no longer shown in search results.
            await page.GetByRole(AriaRole.Link, new() { Name = "Delete Purchase Order" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { NameRegex = new("Delete Purchase Order PO-") }))
                .ToBeVisibleAsync();

            await page.GetByRole(AriaRole.Button, new() { Name = "Yes, Delete Purchase Order" }).ClickAsync();

            await page.WaitForURLAsync("**/purchase-orders");

            await page.Locator("#purchaseOrder-search").FillAsync("AutoID Systems");

            var tableText = await page.Locator("#purchaseOrder-table-body").InnerTextAsync();
            Assert.DoesNotContain(createdOrderNumber, tableText);

        }
    }
}
