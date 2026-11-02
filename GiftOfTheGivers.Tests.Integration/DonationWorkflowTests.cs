using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using GiftOfTheGivers.Tests.Integration.Support;
using Xunit.Abstractions;

namespace GiftOfTheGivers.Tests.Integration
{
    public class DonationWorkflowTests : IClassFixture<GotgWebFactory>
    {
        private readonly GotgWebFactory _factory;
        private readonly ITestOutputHelper _output;

        public DonationWorkflowTests(GotgWebFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task AnonymousDonation_FullWorkflow_IsSavedAndReceiptAndCertificatePagesOpen()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);
            var form = WebHelper.Form(
                ("DonationType", "OneOff"), ("Currency", "ZAR"), ("Amount", "500"),
                ("Cause", "Food Support"), ("IsAnonymous", "true"), ("DonorName", ""));

            // Act: form -> controller -> service -> database
            var post = await WebHelper.PostFormAsync(client, "/Donation", form);
            WebHelper.Log(_output, post);

            // Assert: redirected to the receipt
            Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
            var where = WebHelper.Where(post);
            Assert.Matches(@"^/Donation/Confirmation\?reference=GOTG-\d{4}-\d{6}$", where);

            // Assert: the row really is in the database
            var reference = Regex.Match(where, @"reference=(.+)$").Groups[1].Value;
            var donation = _factory.WithDb(db => db.Donations.Single(d => d.ReferenceNumber == reference));
            Assert.Equal("Anonymous Donor", donation.DonorName);
            Assert.True(donation.IsAnonymous);
            Assert.Equal(500m, donation.Amount);
            Assert.Equal("ZAR", donation.Currency);
            Assert.Equal("Food Support", donation.Cause);
            Assert.Equal("OneOff", donation.DonationType);

            // Assert: receipt page shows the reference
            var receipt = await client.GetAsync(where);
            WebHelper.Log(_output, receipt);
            Assert.Equal(HttpStatusCode.OK, receipt.StatusCode);
            Assert.Contains(reference, await receipt.Content.ReadAsStringAsync());

            // Assert: certificate page opens for that reference
            var certificate = await client.GetAsync($"/Donation/Certificate?reference={reference}");
            WebHelper.Log(_output, certificate);
            Assert.Equal(HttpStatusCode.OK, certificate.StatusCode);
            var html = await certificate.Content.ReadAsStringAsync();
            Assert.Contains("Prototype Tax Certificate", html);
            Assert.Contains(reference, html);
        }

        // Needs the real Azure Function running (locally, or deployed via FunctionsApi__BaseUrl).
        [Fact]
        [Trait("Category", "Functions")]
        public async Task Donation_CertificateNumber_ComesFromTheRealAzureFunction()
        {
            await WebHelper.AssertFunctionsAreRunningAsync(checkStorage: false);   // <-- add this line

            // Arrange
            var client = WebHelper.NewClient(_factory);
            var form = WebHelper.Form(
                ("DonationType", "OneOff"), ("Currency", "ZAR"), ("Amount", "250"),
                ("Cause", "Medical Aid"), ("IsAnonymous", "false"), ("DonorName", "Function Test Donor"));

            // Act
            var post = await WebHelper.PostFormAsync(client, "/Donation", form);
            WebHelper.Log(_output, post);
            Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
            var reference = Regex.Match(WebHelper.Where(post), @"reference=(.+)$").Groups[1].Value;

            // Assert: the function answered and the web app saved its certificate
            var donation = _factory.WithDb(db => db.Donations.Single(d => d.ReferenceNumber == reference));
            Assert.False(string.IsNullOrWhiteSpace(donation.CertificateNumber),
                "No certificate number was saved. The Azure Function did not answer. Is it running, and is FunctionsApi__BaseUrl correct?");
            Assert.Equal($"TC-{reference}", donation.CertificateNumber);
            Assert.Matches("^[0-9A-F]{8}$", donation.CertificateValidationCode);

            // Assert: the certificate page shows the number the function returned
            var certificate = await client.GetAsync($"/Donation/Certificate?reference={reference}");
            WebHelper.Log(_output, certificate);
            Assert.Equal(HttpStatusCode.OK, certificate.StatusCode);
            var html = await certificate.Content.ReadAsStringAsync();
            Assert.Contains("Certificate No. (Azure Function)", html);
            Assert.Contains($"TC-{reference}", html);
            Assert.Contains(donation.CertificateValidationCode!, html);
        }

        [Fact]
        public async Task RecurringUsdDonation_SavesFrequencyAndCurrency()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);
            var form = WebHelper.Form(
                ("DonationType", "Recurring"), ("Frequency", "Weekly"), ("Currency", "USD"),
                ("Amount", "75"), ("Cause", "Medical Aid"),
                ("IsAnonymous", "false"), ("DonorName", "Test Guest"));

            // Act
            var post = await WebHelper.PostFormAsync(client, "/Donation", form);
            WebHelper.Log(_output, post);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
            var reference = Regex.Match(WebHelper.Where(post), @"reference=(.+)$").Groups[1].Value;
            var donation = _factory.WithDb(db => db.Donations.Single(d => d.ReferenceNumber == reference));
            Assert.Equal("Recurring", donation.DonationType);
            Assert.Equal("Weekly", donation.Frequency);
            Assert.Equal("USD", donation.Currency);
            Assert.Equal(75m, donation.Amount);
            Assert.Equal("Test Guest", donation.DonorName);
            Assert.False(donation.IsAnonymous);
        }

        // Edge cases: zero, negative and below-minimum amounts are rejected, nothing saved.
        [Theory]
        [InlineData("0")]
        [InlineData("-50")]
        [InlineData("5")]
        public async Task InvalidAmount_IsRejected_AndNothingIsSaved(string amount)
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);
            var before = _factory.WithDb(db => db.Donations.Count());
            var form = WebHelper.Form(
                ("DonationType", "OneOff"), ("Currency", "ZAR"), ("Amount", amount),
                ("Cause", "General Relief"), ("IsAnonymous", "true"));

            // Act
            var response = await WebHelper.PostFormAsync(client, "/Donation", form);
            WebHelper.Log(_output, response);

            // Assert: the form is shown again with the validation message
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Please enter an amount between 10 and 1,000,000", await response.Content.ReadAsStringAsync());
            Assert.Equal(before, _factory.WithDb(db => db.Donations.Count()));
        }

        [Fact]
        public async Task GuestWithoutNameWhoIsNotAnonymous_GetsFormWithError_AndNothingIsSaved()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);
            var before = _factory.WithDb(db => db.Donations.Count());
            var form = WebHelper.Form(
                ("DonationType", "OneOff"), ("Currency", "ZAR"), ("Amount", "100"),
                ("Cause", "General Relief"), ("IsAnonymous", "false"), ("DonorName", ""));

            // Act
            var response = await WebHelper.PostFormAsync(client, "/Donation", form);
            WebHelper.Log(_output, response);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Please enter your name, or choose to donate anonymously.", await response.Content.ReadAsStringAsync());
            Assert.Equal(before, _factory.WithDb(db => db.Donations.Count()));
        }

        [Fact]
        public async Task UnknownReference_ReturnsNotFound_ForReceiptAndCertificate()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);

            // Act
            var certificate = await client.GetAsync("/Donation/Certificate?reference=GOTG-1999-000000");
            var receipt = await client.GetAsync("/Donation/Confirmation?reference=GOTG-1999-000000");
            WebHelper.Log(_output, certificate);
            WebHelper.Log(_output, receipt);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, certificate.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, receipt.StatusCode);
        }
    }
}
