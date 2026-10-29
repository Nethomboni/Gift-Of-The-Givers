using System.ComponentModel.DataAnnotations.Schema;

namespace GiftOfTheGivers.Web.Models
{
    public class Donation
    {
        #region Identity

        public int Id { get; set; }

        public string ReferenceNumber { get; set; } = Guid.NewGuid().ToString("N");

        public string? UserId { get; set; }

        #endregion

        #region Donation Details

        public string DonationType { get; set; } = "OneOff";

        public string? Frequency { get; set; }

        public string Currency { get; set; } = "ZAR";

        public decimal Amount { get; set; }

        public string Cause { get; set; } = "General Relief";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        #endregion

        #region Donor Info

        public bool IsAnonymous { get; set; }

        public string DonorName { get; set; } = "Anonymous Donor";

        [NotMapped]
        public string DisplayName => IsAnonymous ? "Anonymous Donor" : DonorName;

        #endregion

        #region Certificate Info

        public string? CertificateNumber { get; set; }

        public string? CertificateValidationCode { get; set; }

        #endregion
    }
}