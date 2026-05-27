using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.Models
{
   public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 15 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?-_&])[A-Za-z\d@$!%*?-_&]{8,15}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
