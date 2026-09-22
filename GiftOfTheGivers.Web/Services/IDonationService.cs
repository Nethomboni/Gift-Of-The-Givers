using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Web.Services
{
    public interface IDonationService
    {
        // Saves the donation, then generates and stores a reference number
        // (e.g. GOTG-2026-001245) based on the new row's Id.
        Task<Donation> CreateDonationAsync(DonationViewModel model, string? userId, string? loggedInUserFullName);

        // Called after the GenerateTaxCertificate Azure Function responds, so
        // the certificate number/validation code it issued is persisted
        // against the donation (Part 2, Section A).
        Task SetCertificateInfoAsync(int donationId, string certificateNumber, string validationCode);

        Task<Donation?> GetByReferenceAsync(string referenceNumber);
        Task<List<Donation>> GetByUserAsync(string userId);
        Task<List<Donation>> GetRecentAsync(int count);
        Task<List<Donation>> GetAllAsync();
        Task<decimal> GetTotalAsync();
    }
}
