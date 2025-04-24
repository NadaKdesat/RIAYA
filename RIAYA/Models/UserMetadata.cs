using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RIAYA.Models
{
    public class UserMetadata
    {
        [Required(ErrorMessage = "Full name is required.")]
        [RegularExpression(@"^[a-zA-Z\s]{2,}$", ErrorMessage = "Please enter a valid full name (letters only).")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
            ErrorMessage = "Password must be at least 6 characters and contain an uppercase letter, a lowercase letter, and a number.")]
        public string? PasswordHash { get; set; }

        [NotMapped]
        [Compare("PasswordHash", ErrorMessage = "Passwords do not match.")]
        [Required(ErrorMessage = "Confirm password is required.")]
        public string? ConfirmPassword { get; set; }
    }
}
