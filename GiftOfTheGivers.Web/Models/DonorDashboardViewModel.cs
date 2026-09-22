namespace GiftOfTheGivers.Web.Models
{
    // Display-only aggregation for the Donor Dashboard (Section 17).
    public class DonorDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public decimal TotalDonated { get; set; }
        public int DonationCount { get; set; }
        public Donation? LatestDonation { get; set; }
        public List<Donation> DonationHistory { get; set; } = new();
    }
}
