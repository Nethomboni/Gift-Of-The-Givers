using GiftOfTheGivers.Tests.UI.Pages;
using GiftOfTheGivers.Tests.UI.Support;
using OpenQA.Selenium;

namespace GiftOfTheGivers.Tests.UI
{
    [Trait("Category", "UI")]
    public class LoginTests : UiTestBase
    {
        [Theory]
        [InlineData("chrome")]
        [InlineData("edge")]
        public void Login_ValidEmployee_ReachesDashboard(string browser)
        {
            Start(browser, nameof(Login_ValidEmployee_ReachesDashboard));
            var loginPage = new LoginPage(Driver);

            loginPage.Open();
            loginPage.LoginAs(TestSettings.EmployeeEmail, TestSettings.EmployeePassword);

            Wait().Until(d => d.Url.Contains("/Employee"));
            Assert.True(Driver.FindElement(By.XPath("//h5[contains(.,'Recent Donations')]")).Displayed);
        }

        [Theory]
        [InlineData("chrome")]
        [InlineData("edge")]
        public void Login_WrongPassword_ShowsErrorAndStaysOnLogin(string browser)
        {
            Start(browser, nameof(Login_WrongPassword_ShowsErrorAndStaysOnLogin));
            var loginPage = new LoginPage(Driver);

            loginPage.Open();
            loginPage.LoginAs(TestSettings.EmployeeEmail, "Wrong-Password-123!");

            var error = loginPage.loginError();
            Assert.Contains("Invalid login attempt", error);
            Assert.Contains("/Account/Login", Driver.Url);
        }
    }
}
