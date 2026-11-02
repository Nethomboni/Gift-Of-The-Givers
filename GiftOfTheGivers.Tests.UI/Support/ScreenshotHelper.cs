using OpenQA.Selenium;

namespace GiftOfTheGivers.Tests.UI.Support
{
    public static class ScreenshotHelper
    {
        public static string Save(IWebDriver driver, string name)
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "Screenshots");
            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
            return path;
        }
    }
}
