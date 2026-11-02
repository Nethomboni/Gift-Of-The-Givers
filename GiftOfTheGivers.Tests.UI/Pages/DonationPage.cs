using GiftOfTheGivers.Tests.UI.Support;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GiftOfTheGivers.Tests.UI.Pages
{
    public class DonationPage
    {
        private readonly IWebDriver _driver;

        private readonly By _form = By.Id("donation-form");
        private readonly By _recurringLabel = By.CssSelector("label[for='donationTypeRecurring']");
        private readonly By _frequency = By.Id("Frequency");
        private readonly By _amount = By.Id("Amount");
        private readonly By _currency = By.Id("Currency");
        private readonly By _anonymous = By.Id("IsAnonymous");
        private readonly By _donorName = By.Id("DonorName");
        private readonly By _submit = By.CssSelector("#donation-form button[type='submit']");
        private readonly By _amountError = By.CssSelector("[data-valmsg-for='Amount']");

        public DonationPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void Open()
        {
            _driver.Navigate().GoToUrl(TestSettings.BaseUrl + "/Donation");
            new WebDriverWait(_driver, TimeSpan.FromSeconds(20)).Until(d => d.FindElement(_form).Displayed);
        }

        public void ChooseRecurring(string frequency)
        {
            var label = _driver.FindElement(_recurringLabel);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", label);
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(d => d.FindElement(_frequency).Displayed);
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
                _driver.FindElement(_frequency), frequency);
        }

        public void EnterAmount(string amount)
        {
            var field = _driver.FindElement(_amount);
            field.Clear();
            field.SendKeys(amount);
        }

        public void ChooseCurrency(string currency)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
                _driver.FindElement(_currency), currency);
        }

        public void DonateAnonymously()
        {
            var box = _driver.FindElement(_anonymous);
            if (!box.Selected)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", box);
            }
        }

        public void EnterDonorName(string name)
        {
            var field = _driver.FindElement(_donorName);
            field.Clear();
            field.SendKeys(name);
        }

        public void Submit()
        {
            var button = _driver.FindElement(_submit);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", button);
        }

        public string AmountError()
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => !string.IsNullOrWhiteSpace(d.FindElement(_amountError).Text));
            return _driver.FindElement(_amountError).Text;
        }

        public bool IsStillOnForm()
        {
            return _driver.FindElements(_form).Count > 0;
        }
    }
}