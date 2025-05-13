using System.ComponentModel.DataAnnotations;

namespace RepositoryLayer.DTO
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Old password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Old password must be between 8 and 100 characters.")]
        public string OldPasssword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 100 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Confirm password must be between 8 and 100 characters.")]
        [Compare("NewPassword", ErrorMessage = "Confirm password must match the new password.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}