// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WarehouseManagementSystem.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Display(Name = "Email address")]
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(11, MinimumLength = 11)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "OIB may contain digits only.")]
            [Display(Name = "OIB")]
            public string OIB { get; set; }

            [Required]
            [StringLength(13, MinimumLength = 13)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG may contain digits only.")]
            [Display(Name = "JMBG")]
            public string JMBG { get; set; }
        }

        private async Task LoadAsync(AppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            Email = email;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                OIB = user.OIB,
                JMBG = user.JMBG
            };
        }

        private async Task LoadDisplayDataAsync(AppUser user)
        {
            Username = await _userManager.GetUserNameAsync(user);
            Email = await _userManager.GetEmailAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Profile page could not load user {UserId}", _userManager.GetUserId(User));
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Profile update could not load user {UserId}", _userManager.GetUserId(User));
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDisplayDataAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    _logger.LogWarning("User {User} failed to update phone number", User.Identity?.Name ?? "Unknown");
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.OIB != user.OIB || Input.JMBG != user.JMBG)
            {
                user.OIB = Input.OIB;
                user.JMBG = Input.JMBG;

                var updateProfileResult = await _userManager.UpdateAsync(user);
                if (!updateProfileResult.Succeeded)
                {
                    _logger.LogWarning("User {User} failed to update profile details", User.Identity?.Name ?? "Unknown");

                    foreach (var error in updateProfileResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    await LoadDisplayDataAsync(user);
                    return Page();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User {User} updated profile details", User.Identity?.Name ?? "Unknown");
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("Avatar upload could not load the current user");
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("User {User} tried to upload an empty avatar file", User.Identity?.Name ?? "Unknown");
                return BadRequest("File is required.");
            }

            var allowedContentTypes = new[] {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedContentTypes.Contains(file.ContentType))
            {
                _logger.LogWarning("User {User} tried to upload avatar with invalid content type {ContentType}", User.Identity?.Name ?? "Unknown", file.ContentType);
                return BadRequest("Only JPG, PNG and WEBP images are allowed.");
            }

            const long maxFileSize = 2 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                _logger.LogWarning("User {User} tried to upload avatar larger than allowed size", User.Identity?.Name ?? "Unknown");
                return BadRequest("Maximum file size is 2 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var allowedExtensions = new[] {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("User {User} tried to upload avatar with invalid extension {Extension}", User.Identity?.Name ?? "Unknown", extension);
                return BadRequest("Invalid file extension.");
            }

            DeleteAvatarFile(user);

            var uploadsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "avatars",
                user.Id);

            Directory.CreateDirectory(uploadsPath);

            var storedFileName = Guid.NewGuid() + extension;
            var physicalPath = Path.Combine(uploadsPath, storedFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.AvatarFileName = file.FileName;
            user.AvatarFilePath = "/uploads/avatars/" + user.Id + "/" + storedFileName;
            user.AvatarContentType = file.ContentType;
            user.AvatarFileSize = file.Length;
            user.AvatarUploadedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation(
                "User {User} uploaded avatar {FileName}",
                User.Identity?.Name ?? "Unknown",
                file.FileName);

            return new JsonResult(new { success = true });
        }

        public async Task<PartialViewResult> OnGetAvatarPreviewAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            return new PartialViewResult
            {
                ViewName = "_AvatarPreview",
                ViewData = new ViewDataDictionary<AppUser>(ViewData, user!)
            };
        }

        public async Task<IActionResult> OnPostDeleteAvatarAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("Avatar deletion could not load the current user");
                return NotFound();
            }

            DeleteAvatarFile(user);

            user.AvatarFileName = null;
            user.AvatarFilePath = null;
            user.AvatarContentType = null;
            user.AvatarFileSize = null;
            user.AvatarUploadedAt = null;

            await _userManager.UpdateAsync(user);

            _logger.LogWarning(
                "User {User} deleted profile avatar",
                User.Identity?.Name ?? "Unknown");

            return new JsonResult(new { success = true });
        }

        private static void DeleteAvatarFile(AppUser user)
        {
            if (string.IsNullOrWhiteSpace(user.AvatarFilePath))
            {
                return;
            }

            var relativePath = user.AvatarFilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }
    }
}
