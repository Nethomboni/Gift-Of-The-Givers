using System.ComponentModel.DataAnnotations.Schema;

namespace GiftOfTheGivers.Web.Models
{
    // Persisted record of a (dummy/prototype) donation. Created by DonationService
    // once a DonationViewModel passes validation - see Section 25/38 of the brief:
    // no real payment processing, just a realistic-looking stored record.
    // creating donation class
    public class Donation
    {
        #region Identity

        public int Id { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;

        // Set when the donor was logged in at the time of donation, so it can
        // show up in their Donor Dashboard history. Null for guest donations.
        public string? UserId { get; set; }

        #endregion

        #region Donation Details

        public string DonationType { get; set; } = "OneOff"; // OneOff | Recurring

        public string? Frequency { get; set; } // Weekly | Monthly | Quarterly (Recurring only)

        public string Currency { get; set; } = "ZAR"; // ZAR | USD | EUR

        public decimal Amount { get; set; }

        public string Cause { get; set; } = "General Relief";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        #endregion

        #region Donor Info

        public bool IsAnonymous { get; set; }

        // Snapshot of the donor's display name at the moment of donation - either
        // the logged-in user's full name, a guest's typed name, or "Anonymous Donor".
        // Stored directly (rather than looked up via the User navigation) so it never
        // depends on whether a query included the related ApplicationUser.
        public string DonorName { get; set; } = "Anonymous Donor";

        [NotMapped]
        public string DisplayName => IsAnonymous ? "Anonymous Donor" : DonorName;

        #endregion

        #region Certificate Info

        // Populated (Part 2, Section A) by the GenerateTaxCertificate Azure
        // Function immediately after the donation is created. Null until the
        // function responds - e.g. if the Functions host isn't running - so
        // the Certificate view can still fall back to a locally-generated
        // reference-based number.
        public string? CertificateNumber { get; set; }

        public string? CertificateValidationCode { get; set; }

        #endregion
    }
}
