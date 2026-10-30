using System.Globalization;
using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Web.Helpers
{
    // Pure, side-effect-free helpers so the critical formatting/total logic can be unit tested.
    public static class DonationHelper
    {
        // e.g. id 11 on 2026-09-23 => TC-2026-000011
        public static string FormatTaxCertificateNumber(int donationId, DateTime issuedOn)
            => $"TC-{issuedOn:yyyy}-{donationId:D6}";

        public static decimal CalculateDonationTotal(IEnumerable<Donation>? donations)
            => donations == null ? 0m : Math.Round(donations.Sum(d => d.Amount), 2);

        public static string FormatCurrency(decimal amount, string? currencyCode)
        {
            var code = (currencyCode ?? string.Empty).Trim().ToUpperInvariant();
            var number = amount.ToString("N2", CultureInfo.InvariantCulture);
            return code switch
            {
                "ZAR" => $"R{number}",
                "USD" => $"${number}",
                "EUR" => $"€{number}",
                _ => $"{number} {code}".Trim()
            };
        }
    }
}
