using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        // Where to send the user back to after a successful login (e.g. they were
        // redirected to /Account/Login while trying to reach a protected page).
        public string? ReturnUrl { get; set; }
    }
}
