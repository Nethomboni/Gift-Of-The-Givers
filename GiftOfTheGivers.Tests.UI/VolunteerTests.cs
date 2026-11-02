using GiftOfTheGivers.Tests.UI.Pages;
using GiftOfTheGivers.Tests.UI.Support;

namespace GiftOfTheGivers.Tests.UI
{
    [Trait("Category", "UI")]
    public class VolunteerTests : UiTestBase
    {
        [Theory]
        [InlineData("chrome")]
        [InlineData("edge")]
        public void Register_ValidVolunteer_ShowsConfirmationReference(string browser)
        {
            Start(browser, nameof(Register_ValidVolunteer_ShowsConfirmationReference));
            var volunteerPage = new VolunteerPage(Driver);

            volunteerPage.Open();
            volunteerPage.EnterName("SeleniumTest Volunteer");
            volunteerPage.EnterEmail("seleniumtest@example.com");
            volunteerPage.ChooseSkill("Logistics");
            volunteerPage.ChooseAvailability("Weekends");
            volunteerPage.Submit();

            var reference = volunteerPage.WaitForConfirmationReference();
            Assert.StartsWith("VOL-", reference);
        }

        [Theory]
        [InlineData("chrome")]
        [InlineData("edge")]
        public void Register_BlankName_ShowsValidationError(string browser)
        {
            Start(browser, nameof(Register_BlankName_ShowsValidationError));
            var volunteerPage = new VolunteerPage(Driver);

            volunteerPage.Open();
            volunteerPage.ChooseSkill("Logistics");
            volunteerPage.ChooseAvailability("Weekends");
            volunteerPage.Submit();

            var error = volunteerPage.NameError();
            Assert.Contains("Please enter your full name", error);
            Assert.DoesNotContain("/Volunteer/Confirmation", Driver.Url);
        }
    }
}
