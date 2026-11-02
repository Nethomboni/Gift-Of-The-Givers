using GiftOfTheGivers.Tests.UI.Pages;
using GiftOfTheGivers.Tests.UI.Support;

namespace GiftOfTheGivers.Tests.UI
{
    [Trait("Category", "UI")]
    public class DonationTests : UiTestBase
    {
        [Theory]
        [InlineData("chrome")]
        [InlineData("edge")]
        public void Donate_Anonymously_ShowsReceiptAndCertificate(string browser)
        {
            Start(browser, nameof(Donate_Anonymously_ShowsReceiptAndCertificate));
            var donationPage = new DonationPage(Driver);
            var receipt = new DonationReceiptPage(Driver);

            donationPage.Open();
            donationPage.EnterAmount("50");
            donationPage.DonateAnonymously();
            donationPage.Submit();

            receipt.WaitUntilLoaded();
            var reference = receipt.ReferenceNumber();
            Assert.StartsWith("GOTG-", reference);
            Assert.Contains("Anonymous Donor", receipt.Details());

            var certificate = receipt.OpenCertificate();
            Assert.Contains(reference, certificate);
        }

        [Theory]
        [InlineData("chrome")]
        [InlineData("edge")]
        public void Donate_RecurringMonthlyInUsd_SavesFrequencyAndCurrency(string browser)
        {
            Start(browser, nameof(Donate_RecurringMonthlyInUsd_SavesFrequencyAndCurrency));
            var donationPage = new DonationPage(Driver);
            var receipt = new DonationReceiptPage(Driver);

            donationPage.Open();
            donationPage.ChooseRecurring("Monthly");
            donationPage.EnterAmount("25");
            donationPage.ChooseCurrency("USD");
            donationPage.EnterDonorName("SeleniumTest");
            donationPage.Submit();

            receipt.WaitUntilLoaded();
            var details = receipt.Details();
            Assert.Contains("USD", details);
            Assert.Contains("Recurring (Monthly)", details);
            Assert.Contains("SeleniumTest", details);
        }

        [Theory]
        [InlineData("chrome")]
        [InlineData("edge")]
        public void Donate_ZeroAmount_ShowsValidationErrorAndNoReceipt(string browser)
        {
            Start(browser, nameof(Donate_ZeroAmount_ShowsValidationErrorAndNoReceipt));
            var donationPage = new DonationPage(Driver);

            donationPage.Open();
            donationPage.EnterAmount("0");
            donationPage.DonateAnonymously();
            donationPage.Submit();

            var error = donationPage.AmountError();
            Assert.Contains("10", error);
            Assert.True(donationPage.IsStillOnForm());
            Assert.DoesNotContain("/Donation/Confirmation", Driver.Url);
        }
    }
}