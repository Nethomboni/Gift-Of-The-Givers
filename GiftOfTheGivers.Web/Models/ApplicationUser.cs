using Microsoft.AspNetCore.Identity;

namespace GiftOfTheGivers.Web.Models
{
    // Extends the default Identity user with the display name we need
    // across the Donor Dashboard, Employee Dashboard, and donation records.
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
