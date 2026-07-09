using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace WarehouseManagementSystem.Tests.E2E
{
    public class SupplierPlaywrightTests
    {
        private const string BaseUrl = "https://localhost:44377";
        private const string TestEmail = "dinostupar68@gmail.com";
        private const string TestPassword = "Dino1234!";

        [Fact]
        public async Task SupplierCrud_ShouldWorkThroughBrowser_InTenSteps()
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                SlowMo = 150
            });

            var page = await browser.NewPageAsync();

            var unique = DateTime.Now.ToString("HHmmss");
            var supplierName = $"Playwright Supplier {unique}";
            var editedSupplierName = $"{supplierName} Updated";
            var supplierEmail = $"supplier{unique}@playwright.test";
            var editedSupplierEmail = $"editedsupplier{unique}@playwright.test";

            // 1. Open login page.
            await page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

            // 2. Log in as Admin or Operator.
            await page.Locator("input[name='Input.Email']").FillAsync(TestEmail);
            await page.Locator("input[name='Input.Password']").FillAsync(TestPassword);
            await page.Locator("#login-submit").ClickAsync();

            await page.WaitForURLAsync("**/Home");

            // 3. Open Supplier list page.
            await page.GetByRole(AriaRole.Link, new() { Name = "Suppliers", Exact = true }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Suppliers Registry" }))
                .ToBeVisibleAsync();

            // 4. Open Create Supplier form.
            await page.GetByRole(AriaRole.Link, new() { Name = "+ Create Supplier Item" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Create Supplier" }))
                .ToBeVisibleAsync();

            // 5. Fill supplier form.
            await page.Locator("input[name='Name']").FillAsync(supplierName);
            await page.Locator("input[name='ContactPerson']").FillAsync("Playwright Contact");
            await page.Locator("input[name='ContactEmail']").FillAsync(supplierEmail);
            await page.Locator("input[name='ContactPhone']").FillAsync("+385 91 555 0000");
            await page.Locator("input[name='ContactAddress']").FillAsync("Playwright Supplier Street 20, Zagreb");

            // 6. Submit create form.
            await page.GetByRole(AriaRole.Button, new() { Name = "Create Supplier" }).ClickAsync();

            await page.WaitForURLAsync("**/suppliers");

            // 7. Use AJAX search to find created supplier.
            await page.Locator("#supplier-search").FillAsync(supplierName);

            var createdRow = page.Locator("#supplier-table-body tr")
                .Filter(new() { HasText = supplierName });

            await Expect(createdRow).ToBeVisibleAsync();

            // 8. Open supplier details.
            await createdRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Supplier {supplierName}" }))
                .ToBeVisibleAsync();

            await Expect(page.Locator("body")).ToContainTextAsync(supplierEmail);

            // 9. Edit supplier and verify update.
            await page.GetByRole(AriaRole.Link, new() { Name = "Edit" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Edit Supplier {supplierName}" }))
                .ToBeVisibleAsync();

            await page.Locator("input[name='Name']").FillAsync(editedSupplierName);
            await page.Locator("input[name='ContactPerson']").FillAsync("Updated Playwright Contact");
            await page.Locator("input[name='ContactEmail']").FillAsync(editedSupplierEmail);
            await page.Locator("input[name='ContactPhone']").FillAsync("+385 91 555 9999");
            await page.Locator("input[name='ContactAddress']").FillAsync("Updated Supplier Street 99, Split");

            await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

            await page.WaitForURLAsync("**/suppliers");

            await page.Locator("#supplier-search").FillAsync(editedSupplierName);

            var editedRow = page.Locator("#supplier-table-body tr")
                .Filter(new() { HasText = editedSupplierName });

            await Expect(editedRow).ToBeVisibleAsync();

            await editedRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Supplier {editedSupplierName}" }))
                .ToBeVisibleAsync();

            await Expect(page.Locator("body")).ToContainTextAsync(editedSupplierEmail);
            await Expect(page.Locator("body")).ToContainTextAsync("Updated Playwright Contact");

            // 10. Delete supplier and verify it is no longer in the list.
            await page.GetByRole(AriaRole.Link, new() { Name = "Delete Supplier" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Delete Supplier {editedSupplierName}" }))
                .ToBeVisibleAsync();

            await page.GetByRole(AriaRole.Button, new() { Name = "Yes, Delete Supplier" }).ClickAsync();

            await page.WaitForURLAsync("**/suppliers");

            await page.Locator("#supplier-search").FillAsync(editedSupplierName);

            var tableText = await page.Locator("#supplier-table-body").InnerTextAsync();
            Assert.DoesNotContain(editedSupplierName, tableText);
        }
    }
}
