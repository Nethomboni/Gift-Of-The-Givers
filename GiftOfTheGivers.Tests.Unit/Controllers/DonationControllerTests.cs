using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GiftOfTheGivers.Tests.Unit.Controllers
{
    public class DonationControllerTests
    {
        private readonly Mock<IDonationService> _donationService = new();
        private readonly Mock<IFunctionsApiClient> _functionsApi = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly DonationController _sut;

        public DonationControllerTests()
        {
            _userManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
            _sut = new DonationController(_donationService.Object, _userManager.Object, _functionsApi.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        private static DonationViewModel ValidGuestModel() => new()
        {
            DonationType = "OneOff", Currency = "ZAR", Amount = 100, Cause = "General Relief", DonorName = "Guest Donor"
        };

        [Fact]
        public async Task Index_ValidModel_CallsServiceOnceAndRedirectsToConfirmation()
        {
            // Arrange
            _donationService.Setup(s => s.CreateDonationAsync(It.IsAny<DonationViewModel>(), null, null))
                .ReturnsAsync(new Donation { Id = 1, ReferenceNumber = "GOTG-2026-000001" });
            _functionsApi.Setup(f => f.GenerateTaxCertificateAsync(It.IsAny<Donation>()))
                .ReturnsAsync(((string?)null, (string?)null));
            // Act
            var result = await _sut.Index(ValidGuestModel());
            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Confirmation", redirect.ActionName);
            _donationService.Verify(s => s.CreateDonationAsync(It.IsAny<DonationViewModel>(), null, null), Times.Once);
        }

        [Fact]
        public async Task Index_InvalidModelState_ReturnsSameViewAndNeverCallsService()
        {
            _sut.ModelState.AddModelError("Amount", "Please choose or enter an amount.");
            var model = ValidGuestModel();

            var result = await _sut.Index(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.False(_sut.ModelState.IsValid);
            _donationService.Verify(s => s.CreateDonationAsync(It.IsAny<DonationViewModel>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task Index_GuestNotAnonymousWithoutName_AddsErrorAndNeverCallsService()
        {
            var model = ValidGuestModel();
            model.DonorName = "";

            var result = await _sut.Index(model);

            Assert.IsType<ViewResult>(result);
            Assert.True(_sut.ModelState.ContainsKey(nameof(DonationViewModel.DonorName)));
            _donationService.Verify(s => s.CreateDonationAsync(It.IsAny<DonationViewModel>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task Index_FunctionReturnsCertificate_SavesCertificateInfo()
        {
            _donationService.Setup(s => s.CreateDonationAsync(It.IsAny<DonationViewModel>(), null, null))
                .ReturnsAsync(new Donation { Id = 7, ReferenceNumber = "GOTG-2026-000007" });
            _functionsApi.Setup(f => f.GenerateTaxCertificateAsync(It.IsAny<Donation>()))
                .ReturnsAsync(("TC-GOTG-2026-000007", "VAL42"));

            await _sut.Index(ValidGuestModel());

            _donationService.Verify(s => s.SetCertificateInfoAsync(7, "TC-GOTG-2026-000007", "VAL42"), Times.Once);
        }

        [Fact]
        public async Task Index_RecurringWithoutFrequency_DefaultsToMonthly()
        {
            var model = ValidGuestModel();
            model.DonationType = "Recurring";
            _donationService.Setup(s => s.CreateDonationAsync(It.IsAny<DonationViewModel>(), null, null))
                .ReturnsAsync(new Donation { Id = 1, ReferenceNumber = "R" });

            await _sut.Index(model);

            Assert.Equal("Monthly", model.Frequency);
        }

        [Fact]
        public async Task Confirmation_UnknownReference_ReturnsNotFound()
        {
            _donationService.Setup(s => s.GetByReferenceAsync("X")).ReturnsAsync((Donation?)null);
            Assert.IsType<NotFoundResult>(await _sut.Confirmation("X"));
        }

        [Fact]
        public async Task Confirmation_BlankReference_ReturnsNotFound()
        {
            Assert.IsType<NotFoundResult>(await _sut.Confirmation(""));
        }

        [Fact]
        public async Task Confirmation_KnownReference_ReturnsViewWithDonation()
        {
            var donation = new Donation { ReferenceNumber = "GOTG-2026-000001" };
            _donationService.Setup(s => s.GetByReferenceAsync(donation.ReferenceNumber)).ReturnsAsync(donation);
            var view = Assert.IsType<ViewResult>(await _sut.Confirmation(donation.ReferenceNumber));
            Assert.Same(donation, view.Model);
        }
    }
}
