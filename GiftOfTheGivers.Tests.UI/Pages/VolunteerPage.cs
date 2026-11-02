using GiftOfTheGivers.Tests.UI.Support;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GiftOfTheGivers.Tests.UI.Pages
{
    public class VolunteerPage
    {
        private readonly IWebDriver _driver;

        private readonly By _fullName = By.Id("FullName");
        private readonly By _email = By.Id("Email");
        private readonly By _submit = By.CssSelector("form[action*='/Volunteer'] button[type='submit']");
        private readonly By _nameError = By.CssSelector("[data-valmsg-for='FullName']");
        private readonly By _reference = By.CssSelector("h4");

        public VolunteerPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void Open()
        {
            _driver.Navigate().GoToUrl(TestSettings.BaseUrl + "/Volunteer");
            new WebDriverWait(_driver, TimeSpan.FromSeconds(20)).Until(d => d.FindElement(_fullName).Displayed);
        }

        public void EnterName(string name)
        {
            _driver.FindElement(_fullName).SendKeys(name);
        }

        public void EnterEmail(string email)
        {
            _driver.FindElement(_email).SendKeys(email);
        }

        public void ChooseSkill(string skill)
        {
            ClickChip("skill-" + skill.ToLowerInvariant().Replace(" ", "-"));
        }

        public void ChooseAvailability(string slot)
        {
            ClickChip("avail-" + slot.ToLowerInvariant().Replace(" ", "-"));
        }

        public void Submit()
        {
            var button = _driver.FindElement(_submit);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", button);
        }

        public string NameError()
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => !string.IsNullOrWhiteSpace(d.FindElement(_nameError).Text));
            return _driver.FindElement(_nameError).Text;
        }

        public string WaitForConfirmationReference()
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(30)).Until(d => d.Url.Contains("/Volunteer/Confirmation"));
            return _driver.FindElement(_reference).Text.Trim();
        }

        private void ClickChip(string chipId)
        {
            var label = _driver.FindElement(By.CssSelector($"label[for='{chipId}']"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", label);
            label.Click();
        }
    }
}