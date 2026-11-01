using GiftOfTheGivers.Tests.UI.Pages;
using GiftOfTheGivers.Tests.UI.Support;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;


namespace GiftOfTheGivers.Tests.UI
{
    [Trait("Category", "UI")]
    public class LoginTests : IDisposable
    {
        private IWebDriver? _driver;

        [Theory]
        [InlineData("chrome")]
        [InlineData("edge")]
        public void Login_ValidEmployee_ReachesDashboard(string browser)
        {
            _driver = DriverFactory.Create(browser);
            var loginPage = new LoginPage(_driver);

            loginPage.Open();
            loginPage.LoginAs(TestSettings.EmployeeEmail, TestSettings.EmployeePassword);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("/Employee"));
            Assert.True(_driver.FindElement(By.XPath("//h5[contains(.,'Recent Donations')]")).Displayed);
        }

        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}