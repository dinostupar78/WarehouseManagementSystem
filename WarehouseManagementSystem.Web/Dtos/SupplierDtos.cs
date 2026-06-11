using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Web.Dtos
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string ContactPerson { get; set; } 
        public string ContactEmail { get; set; } 
        public string ContactPhone { get; set; } 
        public string ContactAddress { get; set; } 
        public int PurchaseOrderCount { get; set; }
    }

    public class SupplierCreateDto
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; } 

        [Required]
        [StringLength(120)]
        public string ContactPerson { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(160)]
        public string ContactEmail { get; set; } 

        [Required]
        [Phone]
        [StringLength(40)]
        public string ContactPhone { get; set; } 

        [Required]
        [StringLength(240)]
        public string ContactAddress { get; set; } 
    }

    public class SupplierUpdateDto
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; } 

        [Required]
        [StringLength(120)]
        public string ContactPerson { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(160)]
        public string ContactEmail { get; set; } 

        [Required]
        [Phone]
        [StringLength(40)]
        public string ContactPhone { get; set; } 

        [Required]
        [StringLength(240)]
        public string ContactAddress { get; set; } 
    }

}
