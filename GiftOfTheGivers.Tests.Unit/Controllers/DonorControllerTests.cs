using System.Security.Claims;
using GiftOfTheGivers.Tests.Unit.TestSupport;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GiftOfTheGivers.Tests.Unit.Controllers
{
    public class DonorControllerTests
    {
        private readonly Mock<IDonationService> _donations = new();
        private readonly Mock<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>> _userManager = TestFactory.MockUserManager();
        private readonly DonorController _sut;

        public DonorControllerTests()
        {
            _sut = new DonorController(_donations.Object, _userManager.Object)
            {
                ControllerContext = TestFactory.ControllerContext()
            };
        }

        [Fact]
        public async Task Dashboard_NoCurrentUser_ReturnsChallenge()
        {
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);
            Assert.IsType<ChallengeResult>(await _sut.Dashboard());
        }

        [Fact]
        public async Task Dashboard_UserWithDonations_ReturnsTotalsAndLatest()
        {
            var user = new ApplicationUser { Id = "u1", FullName = "Thandi" };
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var history = new List<Donation>
            {
                new() { Amount = 200m, ReferenceNumber = "LATEST" },
                new() { Amount = 50.50m, ReferenceNumber = "OLDER" }
            };
            _donations.Setup(d => d.GetByUserAsync("u1")).ReturnsAsync(history);

            var view = Assert.IsType<ViewResult>(await _sut.Dashboard());

            var model = Assert.IsType<DonorDashboardViewModel>(view.Model);
            Assert.Equal("Thandi", model.FullName);
            Assert.Equal(2, model.DonationCount);
            Assert.Equal(250.50m, model.TotalDonated);
            Assert.Equal("LATEST", model.LatestDonation!.ReferenceNumber);
        }

        [Fact]
        public async Task Dashboard_UserWithNoDonations_ReturnsZeroTotalAndNoLatest()
        {
            var user = new ApplicationUser { Id = "u2", FullName = "New Donor" };
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _donations.Setup(d => d.GetByUserAsync("u2")).ReturnsAsync(new List<Donation>());

            var view = Assert.IsType<ViewResult>(await _sut.Dashboard());

            var model = Assert.IsType<DonorDashboardViewModel>(view.Model);
            Assert.Equal(0m, model.TotalDonated);
            Assert.Null(model.LatestDonation);
        }
    }
}
