using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GiftOfTheGivers.Tests.UI.Pages
{
    public class DonationReceiptPage
    {
        private readonly IWebDriver _driver;

        private readonly By _reference = By.CssSelector(".receipt-card h4");
        private readonly By _details = By.CssSelector(".receipt-card");
        private readonly By _certificateLink = By.CssSelector("a[href*='/Donation/Certificate']:not([href*='download'])");
        private readonly By _certificateTitle = By.XPath("//h1[contains(.,'Tax Certificate')]");

        public DonationReceiptPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void WaitUntilLoaded()
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(30)).Until(d => d.Url.Contains("/Donation/Confirmation"));
        }

        public string ReferenceNumber()
        {
            return _driver.FindElement(_reference).Text.Trim();
        }

        public string Details()
        {
            return _driver.FindElement(_details).Text;
        }

        public string OpenCertificate()
        {
            var link = _driver.FindElement(_certificateLink);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", link);
            new WebDriverWait(_driver, TimeSpan.FromSeconds(20)).Until(d => d.FindElement(_certificateTitle).Displayed);
            return _driver.PageSource;
        }
    }
}