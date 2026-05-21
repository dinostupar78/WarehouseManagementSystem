namespace WarehouseManagementSystem.Web.Models
{
    public class AutocompleteDropdownModel
    {
        public string FieldName { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string SearchUrl { get; set; } = string.Empty;

        public string Placeholder { get; set; } = "Start typing to search...";

        public string? SelectedValue { get; set; }

        public string? SelectedText { get; set; }

        public string? Prefix { get; set; }

        public bool IsRequired { get; set; } = true;
    }
}
