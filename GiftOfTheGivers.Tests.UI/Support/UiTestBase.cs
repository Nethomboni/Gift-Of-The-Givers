using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GiftOfTheGivers.Tests.UI.Support
{
    public abstract class UiTestBase : IDisposable
    {
        private IWebDriver? _driver;
        private string _screenshotName = "UiTest";

        protected IWebDriver Driver => _driver ?? throw new InvalidOperationException("Call Start before using the driver.");

        protected IWebDriver Start(string browser, string testName)
        {
            _driver = DriverFactory.Create(browser);
            _screenshotName = $"{testName}_{browser}";
            return _driver;
        }

        protected WebDriverWait Wait(int seconds = 20)
        {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
        }

        public void Dispose()
        {
            if (_driver != null)
            {
                ScreenshotHelper.Save(_driver, _screenshotName);
                _driver.Quit();
            }
            GC.SuppressFinalize(this);
        }
    }
}
