using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace WarehouseManagementSystem.Tests.E2E
{
    public class WarehousePlaywrightTests
    {
        private const string BaseUrl = "https://localhost:44377";
        private const string TestEmail = "dinostupar68@gmail.com";
        private const string TestPassword = "Dino1234!";

        [Fact]
        public async Task WarehouseCrud_ShouldWorkThroughBrowser_InTenSteps()
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                SlowMo = 150
            });

            var page = await browser.NewPageAsync();

            var unique = DateTime.Now.ToString("HHmmss");
            var warehouseName = $"Playwright Warehouse {unique}";
            var editedWarehouseName = $"{warehouseName} Updated";

            // 1. Open login page.
            await page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

            // 2. Log in as Admin or Operator.
            await page.Locator("input[name='Input.Email']").FillAsync(TestEmail);
            await page.Locator("input[name='Input.Password']").FillAsync(TestPassword);
            await page.Locator("#login-submit").ClickAsync();

            await page.WaitForURLAsync("**/Home");

            // 3. Open Warehouse list page.
            await page.GetByRole(AriaRole.Link, new() { Name = "Warehouses", Exact = true }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Warehouse Index" }))
                .ToBeVisibleAsync();

            // 4. Open Create Warehouse form.
            await page.GetByRole(AriaRole.Link, new() { Name = "+ Create Warehouse Item" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Create Warehouse" }))
                .ToBeVisibleAsync();

            // 5. Fill warehouse form.
            await page.Locator("input[name='Name']").FillAsync(warehouseName);
            await page.Locator("input[name='Address']").FillAsync("Playwright Street 10");
            await page.Locator("input[name='City']").FillAsync("Zagreb");
            await page.Locator("input[name='Country']").FillAsync("Croatia");
            await page.Locator("input[name='Capacity']").FillAsync("2500");

            // 6. Submit create form.
            await page.GetByRole(AriaRole.Button, new() { Name = "Create Warehouse" }).ClickAsync();
            await page.WaitForURLAsync("**/warehouses");

            // 7. Use AJAX search to find created warehouse.
            await page.Locator("#warehouse-search").FillAsync(warehouseName);

            var createdRow = page.Locator("#warehouse-table-body tr")
                .Filter(new() { HasText = warehouseName });

            await Expect(createdRow).ToBeVisibleAsync();

            // 8. Open details page.
            await createdRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Warehouse {warehouseName}" }))
                .ToBeVisibleAsync();

            // 9. Edit warehouse and verify update.
            await page.GetByRole(AriaRole.Link, new() { Name = "Edit" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Edit Warehouse {warehouseName}" }))
                .ToBeVisibleAsync();

            await page.Locator("input[name='Name']").FillAsync(editedWarehouseName);
            await page.Locator("input[name='City']").FillAsync("Split");
            await page.Locator("input[name='Capacity']").FillAsync("3200");

            await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

            await page.WaitForURLAsync("**/warehouses");

            await page.Locator("#warehouse-search").FillAsync(editedWarehouseName);

            var editedRow = page.Locator("#warehouse-table-body tr")
                .Filter(new() { HasText = editedWarehouseName });

            await Expect(editedRow).ToBeVisibleAsync();

            await editedRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Warehouse {editedWarehouseName}" }))
                .ToBeVisibleAsync();

            await Expect(page.Locator("body")).ToContainTextAsync("Split, Croatia");

            // 10. Delete warehouse and verify it is no longer in the list.
            await page.GetByRole(AriaRole.Link, new() { Name = "Delete Warehouse" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Delete Warehouse {editedWarehouseName}" }))
                .ToBeVisibleAsync();

            await page.GetByRole(AriaRole.Button, new() { Name = "Yes, Delete Warehouse" }).ClickAsync();

            await page.WaitForURLAsync("**/warehouses");

            await page.Locator("#warehouse-search").FillAsync(editedWarehouseName);

            var tableText = await page.Locator("#warehouse-table-body").InnerTextAsync();
            Assert.DoesNotContain(editedWarehouseName, tableText);

        }
    }
}
