using GiftOfTheGivers.Web.Helpers;
using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Tests.Unit.Helpers
{
    public class DonationHelperTests
    {
        [Fact]
        public void FormatTaxCertificateNumber_Id11On2026Sept23_ReturnsTC2026000011()
        {
            // Arrange
            var date = new DateTime(2026, 9, 23);
            // Act
            var result = DonationHelper.FormatTaxCertificateNumber(11, date);
            // Assert
            Assert.Equal("TC-2026-000011", result);
        }

        [Theory]
        [InlineData(1, 2025, "TC-2025-000001")]
        [InlineData(999999, 2030, "TC-2030-999999")]
        public void FormatTaxCertificateNumber_VariousIds_PadsToSixDigits(int id, int year, string expected)
        {
            var result = DonationHelper.FormatTaxCertificateNumber(id, new DateTime(year, 1, 1));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateDonationTotal_EmptyList_ReturnsZero()
        {
            Assert.Equal(0m, DonationHelper.CalculateDonationTotal(new List<Donation>()));
        }

        [Fact]
        public void CalculateDonationTotal_NullList_ReturnsZero()
        {
            Assert.Equal(0m, DonationHelper.CalculateDonationTotal(null));
        }

        [Fact]
        public void CalculateDonationTotal_MultipleDonations_RoundsToTwoDecimals()
        {
            // Arrange
            var donations = new List<Donation>
            {
                new() { Amount = 10.005m },
                new() { Amount = 20.111m }
            };
            // Act
            var total = DonationHelper.CalculateDonationTotal(donations);
            // Assert (30.116 rounds to 30.12)
            Assert.Equal(30.12m, total);
        }

        [Theory]
        [InlineData(1234.5, "ZAR", "R1,234.50")]
        [InlineData(50, "USD", "$50.00")]
        [InlineData(75.25, "EUR", "€75.25")]
        [InlineData(100, "zar", "R100.00")]
        [InlineData(100, "GBP", "100.00 GBP")]
        public void FormatCurrency_KnownAndUnknownCodes_FormatsCorrectly(double amount, string code, string expected)
        {
            var result = DonationHelper.FormatCurrency((decimal)amount, code);
            Assert.Equal(expected, result);
        }
    }
}
