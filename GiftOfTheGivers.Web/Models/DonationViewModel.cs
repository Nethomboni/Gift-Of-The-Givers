using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models
{
    public class DonationViewModel
    {
        [Required]
        public string DonationType { get; set; } = "OneOff"; // OneOff | Recurring

        // Only meaningful when DonationType == "Recurring". Validated in the
        // controller rather than with DataAnnotations, since it's conditionally
        // required and DataAnnotations conditional validation adds more
        // complexity than this prototype needs.
        public string? Frequency { get; set; } // Weekly | Monthly | Quarterly

        [Required]
        public string Currency { get; set; } = "ZAR"; // ZAR | USD | EUR - symbolic only, no conversion (Section 19)

        [Required(ErrorMessage = "Please choose or enter an amount.")]
        [Range(10, 1000000, ErrorMessage = "Please enter an amount between 10 and 1,000,000.")]
        public decimal Amount { get; set; }

        [Required]
        public string Cause { get; set; } = "General Relief";

        public bool IsAnonymous { get; set; }

        [StringLength(150)]
        public string? DonorName { get; set; }

        public static readonly string[] CauseOptions =
        {
            "General Relief",
            "Disaster Relief",
            "Food Support",
            "Medical Aid",
            "Education"
        };

        public static readonly decimal[] PresetAmounts = { 50, 100, 250, 500, 1000 };
    }
}
