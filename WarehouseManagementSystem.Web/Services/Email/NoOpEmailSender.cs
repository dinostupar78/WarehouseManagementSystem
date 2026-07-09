using Microsoft.AspNetCore.Identity.UI.Services;

namespace WarehouseManagementSystem.Web.Services.Email
{
    public class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }
}
