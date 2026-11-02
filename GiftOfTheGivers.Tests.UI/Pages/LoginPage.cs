using GiftOfTheGivers.Tests.UI.Support;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(d => d.FindElement(_email).Displayed);
        }

        public void LoginAs(string email, string password)
        {
            _driver.FindElement(_email).SendKeys(email);
            _driver.FindElement(_password).SendKeys(password);
            _driver.FindElement(_loginButton).Click();
        }
        public string loginError( )
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(d => d.FindElement(_errormessage).Displayed);
            return _driver.FindElement(_errormessage).Text;
        }
    }
    }
