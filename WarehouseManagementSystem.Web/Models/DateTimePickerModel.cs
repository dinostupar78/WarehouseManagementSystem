namespace WarehouseManagementSystem.Web.Models
{
    public class DateTimePickerModel
    {
        public string FieldName { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public DateTime? Value { get; set; }

        public bool IsRequired { get; set; } = true;

        public string? Placeholder { get; set; }
    }
}
