using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace WarehouseManagementSystem.Tests.E2E
{
    public class ProductPlaywrightTests
    {
        private const string BaseUrl = "https://localhost:44377";
        private const string TestEmail = "dinostupar68@gmail.com";
        private const string TestPassword = "Dino1234!";
        private const string ExistingCategoryName = "Office Supplies";

        [Fact]
        public async Task ProductCreate_ShouldWorkThroughBrowser_InTenSteps()
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                SlowMo = 200
            });

            var page = await browser.NewPageAsync();

            var productName = $"Playwright Product {DateTime.Now:HHmmss}";
            var productDescription = "Product created from Playwright E2E test.";

            // 1. Open login page.
            await page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

            // 2. Log in as an existing Admin or Operator user.
            await page.Locator("input[name='Input.Email']").FillAsync(TestEmail);
            await page.Locator("input[name='Input.Password']").FillAsync(TestPassword);
            await page.Locator("#login-submit").ClickAsync();
            await page.WaitForURLAsync("**/Home");
            await Expect(page.GetByText("Warehouse Overview")).ToBeVisibleAsync();

            // 3. Open the Product list page.
            await page.GetByRole(AriaRole.Link, new() { Name = "Products", Exact = true }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Product Index" })).ToBeVisibleAsync();

            // 4. Open the Create Product form.
            await page.GetByRole(AriaRole.Link, new() { Name = "+ Create Product Item" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Create Product" })).ToBeVisibleAsync();

            // 5. Fill basic product data.
            await page.Locator("input[name='Name']").FillAsync(productName);
            await page.Locator("textarea[name='Description']").FillAsync(productDescription);

            // 6. Fill numeric product data.
            await page.Locator("input[name='Price']").FillAsync("25.50");
            await page.Locator("input[name='Weight']").FillAsync("1.25");

            // 7. Fill custom date-time control.
            await page.Locator("[data-datetime-display]").FillAsync("16.03.2026. 09:30");
            await page.Locator("[data-datetime-display]").BlurAsync();

            // 8. Select category using custom AJAX autocomplete dropdown.
            var categoryWidget = page.Locator("[data-autocomplete]").Filter(new()
            {
                Has = page.Locator("input[name='CategoryId']")
            });

            await categoryWidget.Locator("[data-autocomplete-input]").FillAsync(ExistingCategoryName);
            await Expect(page.Locator(".wms-autocomplete-option").First).ToBeVisibleAsync();
            await page.Locator(".wms-autocomplete-option").Filter(new()
            {
                HasText = ExistingCategoryName
            }).First.ClickAsync();

            // 9. Save product.
            await page.GetByRole(AriaRole.Button, new() { Name = "Create Product" }).ClickAsync();
            await page.WaitForURLAsync("**/products");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Product Index" })).ToBeVisibleAsync();
            
            // 10. Search product through AJAX search and open details.
            await page.Locator("#product-search").FillAsync(productName);
            await Expect(page.Locator("#product-table-body").GetByText(productName)).ToBeVisibleAsync();

            var createdRow = page.Locator("#product-table-body tr").Filter(new()
            {
                HasText = productName
            });

            await createdRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = productName, Exact = true })).ToBeVisibleAsync();
            await Expect(page.GetByText(productDescription)).ToBeVisibleAsync();
        }
    }
}
