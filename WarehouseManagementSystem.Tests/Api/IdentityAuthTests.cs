using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManagementSystem.Model;
using WarehouseManagementSystem.Tests.Infrastructure;

namespace WarehouseManagementSystem.Tests.Api
{
    public class IdentityAuthTests : IClassFixture<WarehouseManagementSystemWebApplicationFactory>
    {
        private readonly WarehouseManagementSystemWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public IdentityAuthTests(WarehouseManagementSystemWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task Register_ShouldCreateOperatorUser_WhenInputIsValid()
        {
            await ClearUsersAsync();

            var token = await GetAntiforgeryTokenAsync("/Identity/Account/Register");
            var email = $"operator-{Guid.NewGuid():N}@wms.test";
            var userName = $"operator{Guid.NewGuid():N}"[..20];

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Input.Email"] = email,
                ["Input.UserName"] = userName,
                ["Input.OIB"] = "12345678901",
                ["Input.JMBG"] = "1234567890123",
                ["Input.Password"] = "Test123!",
                ["Input.ConfirmPassword"] = "Test123!"
            };

            var response = await _client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Identity/Account/Manage");

            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = await userManager.FindByEmailAsync(email);

            user.Should().NotBeNull();
            user!.UserName.Should().Be(userName);
            user.OIB.Should().Be("12345678901");
            user.JMBG.Should().Be("1234567890123");
            (await userManager.IsInRoleAsync(user, "Operator")).Should().BeTrue();
        }

        [Fact]
        public async Task Login_ShouldRejectInvalidCredentials()
        {
            await ClearUsersAsync();

            var token = await GetAntiforgeryTokenAsync("/Identity/Account/Login");

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Input.Email"] = "missing@wms.test",
                ["Input.Password"] = "Wrong123!",
                ["Input.RememberMe"] = "false"
            };

            var response = await _client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(form));
            var html = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            html.Should().Contain("Invalid login attempt.");
        }

        [Fact]
        public async Task GuestLogin_ShouldCreateAndSignInGuestUser()
        {
            await ClearUsersAsync();

            var token = await GetAntiforgeryTokenAsync("/Identity/Account/Login");

            var form = new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            };

            var response = await _client.PostAsync("/Identity/Account/Login?handler=Guest", new FormUrlEncodedContent(form));

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Be("/");

            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var guest = await userManager.FindByEmailAsync("guest@wms.local");

            guest.Should().NotBeNull();
            guest!.UserName.Should().Be("guest");
            guest.OIB.Should().Be("00000000000");
            guest.JMBG.Should().Be("0000000000000");
            (await userManager.IsInRoleAsync(guest, "Guest")).Should().BeTrue();
        }

        [Fact]
        public async Task ProtectedAccountPage_ShouldRedirectToLogin_WhenUserIsNotAuthenticated()
        {
            await ClearUsersAsync();

            var response = await _client.GetAsync("/Identity/Account/Manage");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Identity/Account/Login");
        }

        private async Task<string> GetAntiforgeryTokenAsync(string url)
        {
            var response = await _client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var match = Regex.Match(
                html,
                "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<token>[^\"]+)\"");

            match.Success.Should().BeTrue("the form should contain an antiforgery token");

            return WebUtility.HtmlDecode(match.Groups["token"].Value);
        }

        private async Task ClearUsersAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var users = userManager.Users.ToList();

            foreach (var user in users)
            {
                await userManager.DeleteAsync(user);
            }
        }
    }
}
