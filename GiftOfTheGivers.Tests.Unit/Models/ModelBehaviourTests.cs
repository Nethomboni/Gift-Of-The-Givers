using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Tests.Unit.Models
{
    public class ModelBehaviourTests
    {
        private static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, true);
            return results;
        }

        [Theory]
        [InlineData(0, 100, 0)]      // nothing raised
        [InlineData(50, 100, 50)]
        [InlineData(250, 100, 100)]  // capped at 100%
        [InlineData(10, 0, 0)]       // no target set
        public void Project_ProgressPercentage_IsCalculatedAndCapped(double raised, double target, int expected)
        {
            var project = new Project { RaisedAmount = (decimal)raised, TargetAmount = (decimal)target };
            Assert.Equal(expected, project.ProgressPercentage);
        }

        [Fact]
        public void Donation_DisplayName_AnonymousHidesRealName()
        {
            var donation = new Donation { IsAnonymous = true, DonorName = "Real Name" };
            Assert.Equal("Anonymous Donor", donation.DisplayName);
        }

        [Fact]
        public void Donation_DisplayName_NonAnonymousShowsDonorName()
        {
            var donation = new Donation { IsAnonymous = false, DonorName = "Real Name" };
            Assert.Equal("Real Name", donation.DisplayName);
        }

        [Fact]
        public void RegisterViewModel_PasswordMismatch_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                FullName = "A", Email = "a@b.com", Password = "Pass1!", ConfirmPassword = "Different1!"
            };
            Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(RegisterViewModel.ConfirmPassword)));
        }

        [Fact]
        public void RegisterViewModel_ShortPassword_FailsValidation()
        {
            var model = new RegisterViewModel { FullName = "A", Email = "a@b.com", Password = "Ab1!", ConfirmPassword = "Ab1!" };
            Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(RegisterViewModel.Password)));
        }

        [Fact]
        public void ProjectUpdateViewModel_MissingTitleAndDescription_FailsValidation()
        {
            var model = new ProjectUpdateViewModel { ProjectId = 1 };
            var errors = Validate(model);
            Assert.Contains(errors, r => r.MemberNames.Contains(nameof(ProjectUpdateViewModel.Title)));
            Assert.Contains(errors, r => r.MemberNames.Contains(nameof(ProjectUpdateViewModel.Description)));
        }

        [Fact]
        public void ContactViewModel_MissingFields_FailsValidation()
        {
            Assert.True(Validate(new ContactViewModel()).Count >= 4);
        }
    }
}
