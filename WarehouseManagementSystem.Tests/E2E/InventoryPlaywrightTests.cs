using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace WarehouseManagementSystem.Tests.E2E
{
    public class InventoryPlaywrightTests
    {
        private const string BaseUrl = "https://localhost:44377";
        private const string TestEmail = "dinostupar68@gmail.com";
        private const string TestPassword = "Dino1234!";

        [Fact]
        public async Task InventoryCrud_ShouldWorkThroughBrowser_InTenSteps()
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                SlowMo = 150
            });

            var page = await browser.NewPageAsync();

            const string productName = "Laptop";
            const string locationCode = "MDC-A-01";
            const string createdQuantity = "18";
            const string editedQuantity = "27";

            // 1. Open login page.
            await page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

            // 2. Log in as Admin or Operator.
            await page.Locator("input[name='Input.Email']").FillAsync(TestEmail);
            await page.Locator("input[name='Input.Password']").FillAsync(TestPassword);
            await page.Locator("#login-submit").ClickAsync();

            await page.WaitForURLAsync("**/Home");

            // 3. Open Inventory list page.
            await page.GetByRole(AriaRole.Link, new() { Name = "Inventory", Exact = true }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Inventory Control" }))
                .ToBeVisibleAsync();

            // 4. Open Create Inventory form.
            await page.GetByRole(AriaRole.Link, new() { Name = "+ Create Inventory Item" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Create Inventory Item" }))
                .ToBeVisibleAsync();

            // 5. Select product through custom AJAX autocomplete.
            var productAutocomplete = page.Locator("[data-autocomplete]").Nth(0);

            await productAutocomplete
                .Locator("[data-autocomplete-input]")
                .FillAsync(productName);

            var firstProductOption = productAutocomplete
                .Locator(".wms-autocomplete-option")
                .First;

            await Expect(firstProductOption).ToBeVisibleAsync();
            await firstProductOption.ClickAsync();

            // 6. Select location through custom AJAX autocomplete.
            var locationAutocomplete = page.Locator("[data-autocomplete]").Nth(1);

            await locationAutocomplete
                .Locator("[data-autocomplete-input]")
                .FillAsync(locationCode);

            var firstLocationOption = locationAutocomplete
                .Locator(".wms-autocomplete-option")
                .First;

            await Expect(firstLocationOption).ToBeVisibleAsync();
            await firstLocationOption.ClickAsync();

            // 7. Fill custom date-time control and quantity.
            await page.Locator("[data-datetime-display]").FillAsync("16.03.2026. 08:30");
            await page.Locator("input[name='Quantity']").FillAsync(createdQuantity);

            // 8. Submit create form and verify with AJAX search.
            await page.GetByRole(AriaRole.Button, new() { Name = "Create Inventory Item" }).ClickAsync();

            await page.WaitForURLAsync("**/inventories");

            await page.Locator("#inventory-search").FillAsync(productName);

            var createdRow = page.Locator("#inventory-table-body tr")
                .Filter(new() { HasText = productName })
                .Filter(new() { HasText = locationCode })
                .Filter(new() { HasText = $"{createdQuantity} Units" });

            await Expect(createdRow).ToBeVisibleAsync();

            // 9. Open details, edit quantity and verify update.
            await createdRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.Locator("body")).ToContainTextAsync(productName);
            await Expect(page.Locator("body")).ToContainTextAsync(locationCode);
            await Expect(page.Locator("body")).ToContainTextAsync($"{createdQuantity} Units");

            await page.GetByRole(AriaRole.Link, new() { Name = "Edit Inventory" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { NameRegex = new("Edit Inventory") }))
                .ToBeVisibleAsync();

            await page.Locator("input[name='Quantity']").FillAsync(editedQuantity);
            await page.Locator("[data-datetime-display]").FillAsync("17.03.2026. 09:45");

            await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

            await page.WaitForURLAsync("**/inventories");

            await page.Locator("#inventory-search").FillAsync(productName);

            var editedRow = page.Locator("#inventory-table-body tr")
                .Filter(new() { HasText = productName })
                .Filter(new() { HasText = locationCode })
                .Filter(new() { HasText = $"{editedQuantity} Units" });

            await Expect(editedRow).ToBeVisibleAsync();

            await editedRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.Locator("body")).ToContainTextAsync(productName);
            await Expect(page.Locator("body")).ToContainTextAsync(locationCode);
            await Expect(page.Locator("body")).ToContainTextAsync($"{editedQuantity} Units");

            // 10. Delete inventory item and verify it is no longer shown in search results.
            await page.GetByRole(AriaRole.Link, new() { Name = "Delete Inventory" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { NameRegex = new("Delete Inventory") }))
                .ToBeVisibleAsync();

            await page.GetByRole(AriaRole.Button, new() { Name = "Yes, Delete Inventory" }).ClickAsync();

            await page.WaitForURLAsync("**/inventories");

            await page.Locator("#inventory-search").FillAsync(productName);

            var tableText = await page.Locator("#inventory-table-body").InnerTextAsync();
            Assert.DoesNotContain($"{editedQuantity} Units", tableText);


        }
    }
}
