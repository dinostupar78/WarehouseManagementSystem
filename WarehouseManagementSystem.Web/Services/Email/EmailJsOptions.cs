namespace WarehouseManagementSystem.Web.Services.Email
{
    public class EmailJsOptions
    {
        public string ServiceId { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string? PrivateKey { get; set; }
    }
}
