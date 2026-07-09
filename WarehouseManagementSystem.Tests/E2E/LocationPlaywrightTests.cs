using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace WarehouseManagementSystem.Tests.E2E
{
    public class LocationPlaywrightTests
    {
        private const string BaseUrl = "https://localhost:44377";
        private const string TestEmail = "dinostupar68@gmail.com";
        private const string TestPassword = "Dino1234!";

        [Fact]
        public async Task LocationCrud_ShouldWorkThroughBrowser_InTenSteps()
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                SlowMo = 150
            });

            var page = await browser.NewPageAsync();

            var unique = DateTime.Now.ToString("HHmmss");
            var locationCode = $"PLW-A-{unique}";
            var editedLocationCode = $"PLW-B-{unique}";

            // 1. Open login page.
            await page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

            // 2. Log in as Admin or Operator.
            await page.Locator("input[name='Input.Email']").FillAsync(TestEmail);
            await page.Locator("input[name='Input.Password']").FillAsync(TestPassword);
            await page.Locator("#login-submit").ClickAsync();

            await page.WaitForURLAsync("**/Home");

            // 3. Open Location list page.
            await page.GetByRole(AriaRole.Link, new() { Name = "Locations", Exact = true }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Allocated Locations" }))
                .ToBeVisibleAsync();

            // 4. Open Create Location form.
            await page.GetByRole(AriaRole.Link, new() { Name = "+ Create Location Item" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Create Location" }))
                .ToBeVisibleAsync();

            // 5. Fill regular location fields.
            await page.Locator("input[name='Code']").FillAsync(locationCode);
            await page.Locator("input[name='Zone']").FillAsync("A");
            await page.Locator("input[name='ShelfNumber']").FillAsync("3");

            // 6. Select warehouse through custom AJAX autocomplete dropdown.
            var warehouseAutocomplete = page.Locator("[data-autocomplete]").First;

            await warehouseAutocomplete
                .Locator("[data-autocomplete-input]")
                .FillAsync("Main Distribution Center");

            var firstWarehouseOption = warehouseAutocomplete
                .Locator(".wms-autocomplete-option")
                .First;

            await Expect(firstWarehouseOption).ToBeVisibleAsync();
            await firstWarehouseOption.ClickAsync();

            // 7. Submit create form.
            await page.GetByRole(AriaRole.Button, new() { Name = "Create Location" }).ClickAsync();

            await page.WaitForURLAsync("**/locations");

            // 8. Use AJAX search to find created location.
            await page.Locator("#location-search").FillAsync(locationCode);

            var createdRow = page.Locator("#location-table-body tr")
                .Filter(new() { HasText = locationCode });

            await Expect(createdRow).ToBeVisibleAsync();

            await createdRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Location {locationCode}" }))
                .ToBeVisibleAsync();

            await Expect(page.Locator("body")).ToContainTextAsync("Main Distribution Center");

            // 9. Edit location and verify update.
            await page.GetByRole(AriaRole.Link, new() { Name = "Edit" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Edit Location {locationCode}" }))
                .ToBeVisibleAsync();

            await page.Locator("input[name='Code']").FillAsync(editedLocationCode);
            await page.Locator("input[name='Zone']").FillAsync("B");
            await page.Locator("input[name='ShelfNumber']").FillAsync("7");

            await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();

            await page.WaitForURLAsync("**/locations");

            await page.Locator("#location-search").FillAsync(editedLocationCode);

            var editedRow = page.Locator("#location-table-body tr")
                .Filter(new() { HasText = editedLocationCode });

            await Expect(editedRow).ToBeVisibleAsync();

            await editedRow.GetByRole(AriaRole.Link, new() { Name = "DETAILS" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Location {editedLocationCode}" }))
                .ToBeVisibleAsync();

            await Expect(page.Locator("body")).ToContainTextAsync(editedLocationCode);
            await Expect(page.Locator("body")).ToContainTextAsync("Main Distribution Center");
            await Expect(page.Locator("body")).ToContainTextAsync("Shelf Reference");
            await Expect(page.Locator("body")).ToContainTextAsync("7");

            // 10. Delete location and verify it is no longer in the list.
            await page.GetByRole(AriaRole.Link, new() { Name = "Delete Location" }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = $"Delete Location {editedLocationCode}" }))
                .ToBeVisibleAsync();

            await page.GetByRole(AriaRole.Button, new() { Name = "Yes, Delete Location" }).ClickAsync();

            await page.WaitForURLAsync("**/locations");

            await page.Locator("#location-search").FillAsync(editedLocationCode);

            var tableText = await page.Locator("#location-table-body").InnerTextAsync();
            Assert.DoesNotContain(editedLocationCode, tableText);

        }
    }
}
