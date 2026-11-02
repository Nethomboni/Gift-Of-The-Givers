using GiftOfTheGivers.Tests.UI.Support;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GiftOfTheGivers.Tests.UI.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;

        private readonly By _email = By.Id("Email");
        private readonly By _password = By.Id("Password");
        private readonly By _loginButton = By.CssSelector("form button[type='submit']");
        private readonly By _errormessage = By.CssSelector(".validation-summary-errors li");

        public LoginPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void Open()
        {
            _driver.Navigate().GoToUrl(TestSettings.BaseUrl + "/Account/Login");
            new WebDriverWait(_driver, TimeSpan.FromSeconds(20)).Until(d => d.FindElement(_email).Displayed);
        }

        public void LoginAs(string email, string password)
        {
            _driver.FindElement(_email).SendKeys(email);
            _driver.FindElement(_password).SendKeys(password);
            var button = _driver.FindElement(_loginButton);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", button);
        }

        public string loginError()
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(20)).Until(d => d.FindElement(_errormessage).Displayed);
            return _driver.FindElement(_errormessage).Text;
        }
    }
}