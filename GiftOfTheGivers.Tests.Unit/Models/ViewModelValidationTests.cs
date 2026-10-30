using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Tests.Unit.Models
{
    public class ViewModelValidationTests
    {
        private static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
            return results;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        [InlineData(9)]
        [InlineData(1000001)]
        public void DonationViewModel_InvalidAmount_FailsValidation(double amount)
        {
            var model = new DonationViewModel { Amount = (decimal)amount };
            Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(DonationViewModel.Amount)));
        }

        [Fact]
        public void DonationViewModel_ValidAmount_PassesValidation()
        {
            Assert.Empty(Validate(new DonationViewModel { Amount = 100 }));
        }

        [Fact]
        public void DonationViewModel_MissingRequiredStrings_FailsValidation()
        {
            var model = new DonationViewModel { Amount = 100, DonationType = null!, Currency = null!, Cause = null! };
            Assert.True(Validate(model).Count >= 3);
        }

        [Fact]
        public void VolunteerViewModel_MissingFullName_FailsValidation()
        {
            var model = new VolunteerViewModel { FullName = "" };
            Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(VolunteerViewModel.FullName)));
        }

        [Fact]
        public void VolunteerViewModel_InvalidEmail_FailsValidation()
        {
            var model = new VolunteerViewModel { FullName = "Test", Email = "not-an-email" };
            Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(VolunteerViewModel.Email)));
        }

        [Fact]
        public void VolunteerViewModel_ValidDetails_PassesValidation()
        {
            Assert.Empty(Validate(new VolunteerViewModel { FullName = "Test", Email = "a@b.com" }));
        }
    }
}
