using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace WarehouseManagementSystem.Tests.E2E
{
    public class CategoryPlaywrightTests
    {
        private const string BaseUrl = "https://localhost:44377";
        private const string TestEmail = "dinostupar68@gmail.com";
        private const string TestPassword = "Dino1234!";

        [Fact]
        public async Task CategoryCrud_ShouldWorkThroughBrowser_InTenSteps()
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                SlowMo = 200
            });

            var page = await browser.NewPageAsync();

            var categoryName = $"Playwright Category {DateTime.Now:HHmmss}";
            var categoryDescription = "Category created from Playwright test.";
            var updatedDescription = "Category updated from Playwright test.";

            // 1. Open login page.
            await page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

            // 2. Log in as an existing Admin or Operator user.
            await page.Locator("input[name='Input.Email']").FillAsync(TestEmail);
            await page.Locator("input[name='Input.Password']").FillAsync(TestPassword);
            await page.Locator("#login-submit").ClickAsync();
            await page.WaitForURLAsync("**/Home");
            await Expect(page.GetByText("Warehouse Overview")).ToBeVisibleAsync();

            // 3. Open the Category list page.
            await page.GetByRole(AriaRole.Link, new() { Name = "Categories" }).ClickAsync();
            await Expect(page.GetByText("Category Management")).ToBeVisibleAsync();

            // 4. Open the Create Category form.
            await page.GetByRole(AriaRole.Link, new() { Name = "+ Create Category" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Create Category" })).ToBeVisibleAsync();

            // 5. Fill and submit the Create Category form.
            await page.Locator("input[name='Name']").FillAsync(categoryName);
            await page.Locator("textarea[name='Description']").FillAsync(categoryDescription);
            await page.GetByRole(AriaRole.Button, new() { Name = "Create Category" }).ClickAsync();
            await page.WaitForURLAsync("**/categories");

            // 6. Search for the created category using AJAX search.
            await page.Locator("#category-search").FillAsync(categoryName);
            await Expect(page.GetByText(categoryName)).ToBeVisibleAsync();

            // 7. Open the created category details page.
            var createdRow = page.Locator("#category-table-body tr").Filter(new()
            {
                HasText = categoryName
            });

            await createdRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();
            await Expect(page.GetByText($"Category {categoryName}")).ToBeVisibleAsync();

            // 8. Edit the category description.
            await page.Locator("a[href*='/edit']").ClickAsync();
            await page.Locator("textarea[name='Description']").FillAsync(updatedDescription);
            await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();
            await page.WaitForURLAsync("**/categories");

            // Open the updated category details page again because edit redirects back to index.
            await page.Locator("#category-search").FillAsync(categoryName);
            await Expect(page.GetByText(categoryName)).ToBeVisibleAsync();

            var updatedRow = page.Locator("#category-table-body tr").Filter(new()
            {
                HasText = categoryName
            });

            await updatedRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();
            await Expect(page.GetByText(updatedDescription)).ToBeVisibleAsync();

            // 9. Delete the category.
            await page.GetByRole(AriaRole.Link, new() { Name = "Delete Category" }).ClickAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Yes, Delete Category" }).ClickAsync();
            await page.WaitForURLAsync("**/categories");

            // 10. Confirm the category is no longer visible in the AJAX search results.
            await page.Locator("#category-search").FillAsync(categoryName);
            await Expect(page.GetByText("No categories mapped.")).ToBeVisibleAsync();

        }
    }
}
