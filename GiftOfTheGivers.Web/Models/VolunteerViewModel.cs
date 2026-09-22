using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models
{
    public class VolunteerViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        // Bound from a checkbox group (Section 29). "At least one selected" is
        // checked in the controller - [Required] on a List<string> only checks
        // the list isn't null, not that it's non-empty.
        public List<string> SelectedSkills { get; set; } = new();

        // Bound from a checkbox group (Section 30).
        public List<string> SelectedAvailability { get; set; } = new();

        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string? Phone { get; set; }

        public static readonly string[] SkillOptions =
        {
            "Medical", "IT", "Logistics", "Administration", "Teaching",
            "Driving", "Food Distribution", "Fundraising", "Social Media"
        };

        public static readonly string[] AvailabilityOptions =
        {
            "Weekdays", "Weekends", "Evenings", "Emergency Response", "Flexible"
        };
    }
}
