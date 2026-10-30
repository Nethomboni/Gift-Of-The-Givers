using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GiftOfTheGivers.Tests.Unit.UI
{
    // Shared Chrome session for all Selenium tests (xUnit class fixture).
    // Start the web app first, then set GOTG_BASE_URL (defaults to https://localhost:7000).
    public class SeleniumFixture : IDisposable
    {
        public IWebDriver Driver { get; }
        public string BaseUrl { get; } =
            (Environment.GetEnvironmentVariable("GOTG_BASE_URL") ?? "https://localhost:7000").TrimEnd('/');

        public SeleniumFixture()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");          // needed on CI agents
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--window-size=1280,900");
            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public void Dispose()
        {
            Driver.Quit();
            Driver.Dispose();
        }
    }
}
