namespace GiftOfTheGivers.Web.Models
{
    // Persisted record of a volunteer sign-up. Skills and Availability are stored
    // as comma-separated values from the checkbox groups on the Volunteer form -
    // simple storage is fine at prototype stage (Section 32).
    public class Volunteer
    {
        public int Id { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Skills { get; set; } = string.Empty;

        public string Availability { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // New | Reviewing | Contacted - prototype-only statuses (Section 33)
        public string Status { get; set; } = "New";
    }
}
