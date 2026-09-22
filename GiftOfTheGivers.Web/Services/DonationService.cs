using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Services
{
    public class DonationService : IDonationService
    {
        private readonly ApplicationDbContext _context;

        public DonationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Donation> CreateDonationAsync(DonationViewModel model, string? userId, string? loggedInUserFullName)
        {
            string donorName;
            if (model.IsAnonymous)
            {
                donorName = "Anonymous Donor";
            }
            else if (!string.IsNullOrWhiteSpace(loggedInUserFullName))
            {
                donorName = loggedInUserFullName;
            }
            else
            {
                donorName = string.IsNullOrWhiteSpace(model.DonorName) ? "Anonymous Donor" : model.DonorName!.Trim();
            }

            var donation = new Donation
            {
                UserId = userId,
                DonationType = model.DonationType,
                Frequency = model.DonationType == "Recurring" ? model.Frequency : null,
                Currency = model.Currency,
                Amount = model.Amount,
                Cause = model.Cause,
                IsAnonymous = model.IsAnonymous,
                DonorName = donorName,
                CreatedAt = DateTime.Now
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // Reference is derived from the generated Id so it's guaranteed unique
            // without a separate counter table - e.g. GOTG-2026-001245.
            donation.ReferenceNumber = $"GOTG-{donation.CreatedAt:yyyy}-{donation.Id:D6}";
            await _context.SaveChangesAsync();

            return donation;
        }

        public async Task SetCertificateInfoAsync(int donationId, string certificateNumber, string validationCode)
        {
            var donation = await _context.Donations.FindAsync(donationId);
            if (donation == null)
            {
                return;
            }

            donation.CertificateNumber = certificateNumber;
            donation.CertificateValidationCode = validationCode;
            await _context.SaveChangesAsync();
        }

        public async Task<Donation?> GetByReferenceAsync(string referenceNumber)
        {
            return await _context.Donations
                .FirstOrDefaultAsync(d => d.ReferenceNumber == referenceNumber);
        }

        public async Task<List<Donation>> GetByUserAsync(string userId)
        {
            return await _context.Donations
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Donation>> GetRecentAsync(int count)
        {
            return await _context.Donations
                .OrderByDescending(d => d.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Donation>> GetAllAsync()
        {
            return await _context.Donations
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAsync()
        {
            return await _context.Donations.SumAsync(d => (decimal?)d.Amount) ?? 0m;
        }
    }
}
