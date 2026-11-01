using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace GiftOfTheGivers.Tests.UI.Support
{
    public static class DriverFactory
    {
        public static IWebDriver Create(string browser, int width = 1280, int height = 900)
        {
            bool headless = Environment.GetEnvironmentVariable("GOTG_HEADLESS") == "true";

            switch (browser.ToLowerInvariant())
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument($"--window-size={width},{height}");
                    if (headless) chromeOptions.AddArgument("--headless=new");
                    return new ChromeDriver(chromeOptions);

                case "edge":
                    var edgeOptions = new EdgeOptions();
                    edgeOptions.AddArgument($"--window-size={width},{height}");
                    if (headless) edgeOptions.AddArgument("--headless=new");
                    return new EdgeDriver(edgeOptions);

                default:
                    throw new ArgumentException($"Unsupported browser: {browser}");
            }
        }
    }
}
