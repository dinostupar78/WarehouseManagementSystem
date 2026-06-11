using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagementSystem.Model
{
    public class AppUser : IdentityUser
    {
        [Required]
        [StringLength(11, MinimumLength = 11)]
        [RegularExpression("^[0-9]*$")]
        public string OIB { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 13)]
        [RegularExpression("^[0-9]*$")]
        public string JMBG { get; set; }

        public string? AvatarFileName { get; set; }
        public string? AvatarFilePath { get; set; }
        public string? AvatarContentType { get; set; }
        public long? AvatarFileSize { get; set; }
        public DateTime? AvatarUploadedAt { get; set; }
    }
}
