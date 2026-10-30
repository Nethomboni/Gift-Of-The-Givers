using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GiftOfTheGivers.Tests.Unit.Controllers
{
    public class VolunteerControllerTests
    {
        private readonly Mock<IVolunteerService> _service = new();
        private readonly VolunteerController _sut;

        public VolunteerControllerTests() => _sut = new VolunteerController(_service.Object);

        private static VolunteerViewModel Valid() => new()
        {
            FullName = "Test Volunteer",
            SelectedSkills = new() { "IT" },
            SelectedAvailability = new() { "Weekends" }
        };

        [Fact]
        public async Task Index_ValidModel_CallsServiceOnceAndRedirects()
        {
            _service.Setup(s => s.CreateVolunteerAsync(It.IsAny<VolunteerViewModel>()))
                .ReturnsAsync(new Volunteer { ReferenceNumber = "VOL-2026-00001" });

            var result = await _sut.Index(Valid());

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Confirmation", redirect.ActionName);
            _service.Verify(s => s.CreateVolunteerAsync(It.IsAny<VolunteerViewModel>()), Times.Once);
        }

        [Fact]
        public async Task Index_NoSkillsSelected_ReturnsViewWithErrorAndNeverCallsService()
        {
            var model = Valid();
            model.SelectedSkills.Clear();

            var result = await _sut.Index(model);

            Assert.IsType<ViewResult>(result);
            Assert.True(_sut.ModelState.ContainsKey(nameof(VolunteerViewModel.SelectedSkills)));
            _service.Verify(s => s.CreateVolunteerAsync(It.IsAny<VolunteerViewModel>()), Times.Never);
        }

        [Fact]
        public async Task Index_NoAvailabilitySelected_ReturnsViewAndNeverCallsService()
        {
            var model = Valid();
            model.SelectedAvailability.Clear();

            var result = await _sut.Index(model);

            Assert.IsType<ViewResult>(result);
            _service.Verify(s => s.CreateVolunteerAsync(It.IsAny<VolunteerViewModel>()), Times.Never);
        }

        [Fact]
        public async Task Index_InvalidModelState_ReturnsSameView()
        {
            _sut.ModelState.AddModelError("FullName", "Please enter your full name.");
            var model = Valid();
            var view = Assert.IsType<ViewResult>(await _sut.Index(model));
            Assert.Same(model, view.Model);
        }

        [Fact]
        public async Task Confirmation_UnknownReference_ReturnsNotFound()
        {
            _service.Setup(s => s.GetByReferenceAsync("X")).ReturnsAsync((Volunteer?)null);
            Assert.IsType<NotFoundResult>(await _sut.Confirmation("X"));
        }
    }
}
