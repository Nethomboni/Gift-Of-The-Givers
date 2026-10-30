using OpenQA.Selenium;

namespace GiftOfTheGivers.Tests.Unit.UI
{
    // Selenium tests written with xUnit. Tagged "UI" so unit-test runs and the CI
    // coverage run can skip them:  dotnet test --filter "Category!=UI"
    // Run them locally with the site running:  dotnet test --filter "Category=UI"
    [Trait("Category", "UI")]
    public class WebUiTests : IClassFixture<SeleniumFixture>
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;

        public WebUiTests(SeleniumFixture fixture)
        {
            _driver = fixture.Driver;
            _baseUrl = fixture.BaseUrl;
        }

        [Fact]
        public void HomePage_Loads_TitleContainsHome()
        {
            _driver.Navigate().GoToUrl(_baseUrl + "/");
            Assert.Contains("Home", _driver.Title);
        }

        [Fact]
        public void VolunteerForm_SubmitEmpty_ShowsValidationMessages()
        {
            _driver.Navigate().GoToUrl(_baseUrl + "/Volunteer");
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            var errors = _driver.FindElements(By.CssSelector(".text-danger"));
            Assert.Contains(errors, e => !string.IsNullOrWhiteSpace(e.Text));
        }

        [Fact]
        public void VolunteerForm_ValidSubmission_RedirectsToConfirmation()
        {
            _driver.Navigate().GoToUrl(_baseUrl + "/Volunteer");
            _driver.FindElement(By.Id("FullName")).SendKeys("Selenium Tester");
            _driver.FindElement(By.CssSelector("input[name='SelectedSkills']")).FindElement(By.XPath("following-sibling::label")).Click();
            _driver.FindElement(By.CssSelector("input[name='SelectedAvailability']")).FindElement(By.XPath("following-sibling::label")).Click();
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            Assert.Contains("/Volunteer/Confirmation", _driver.Url);
        }

        [Fact]
        public void DonationForm_PresetAmountThenSubmit_ReachesConfirmation()
        {
            _driver.Navigate().GoToUrl(_baseUrl + "/Donation");
            _driver.FindElement(By.CssSelector("[data-amount-btn='100']")).Click();
            _driver.FindElement(By.Id("DonorName")).SendKeys("Selenium Donor");
            _driver.FindElement(By.CssSelector("#donation-form button[type='submit']")).Click();
            Assert.Contains("/Donation/Confirmation", _driver.Url);
        }
    }
}
