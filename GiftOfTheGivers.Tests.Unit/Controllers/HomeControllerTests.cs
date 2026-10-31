
using GiftOfTheGivers.Tests.Unit.TestSupport;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GiftOfTheGivers.Tests.Unit.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<IVolunteerService> _volunteers = new();

        private HomeController CreateSut(Web.Data.ApplicationDbContext context)
        {
            var cache = new MemoryCache(new MemoryCacheOptions());

            return new HomeController(context, _volunteers.Object, cache)
            {
                ControllerContext = TestFactory.ControllerContext(),
                TempData = TestFactory.TempData()
            };
        }

        [Fact]
        public async Task Index_WithProjects_ReturnsCountsAndMaxThreeFeaturedOngoing()
        {
            // Arrange
            var context = TestFactory.CreateContext();
            for (var i = 1; i <= 4; i++)
                context.Projects.Add(new Project { Name = $"P{i}", Status = "Ongoing", CreatedAt = DateTime.Now.AddDays(-i) });
            context.Projects.Add(new Project { Name = "Done", Status = "Completed" });
            await context.SaveChangesAsync();
            _volunteers.Setup(v => v.GetCountAsync()).ReturnsAsync(12);

            // Act
            var view = Assert.IsType<ViewResult>(await CreateSut(context).Index());

            // Assert
            var model = Assert.IsType<HomeIndexViewModel>(view.Model);
            Assert.Equal(12, model.VolunteersCount);
            Assert.Equal(4, model.ActiveProjectsCount);
            Assert.Equal(3, model.FeaturedProjects.Count);
            Assert.All(model.FeaturedProjects, p => Assert.Equal("Ongoing", p.Status));
        }

        [Fact]
        public void About_ReturnsView()
        {
            Assert.IsType<ViewResult>(CreateSut(TestFactory.CreateContext()).About());
        }

        [Fact]
        public void Contact_Get_ReturnsViewWithEmptyModel()
        {
            var view = Assert.IsType<ViewResult>(CreateSut(TestFactory.CreateContext()).Contact());
            Assert.IsType<ContactViewModel>(view.Model);
        }

        [Fact]
        public void Contact_InvalidModel_ReturnsSameView()
        {
            var sut = CreateSut(TestFactory.CreateContext());
            sut.ModelState.AddModelError("Name", "Required");
            var model = new ContactViewModel();

            var view = Assert.IsType<ViewResult>(sut.Contact(model));

            Assert.Same(model, view.Model);
        }

        [Fact]
        public void Contact_ValidModel_SetsSuccessMessageAndRedirects()
        {
            var sut = CreateSut(TestFactory.CreateContext());
            var model = new ContactViewModel { Name = "A", Email = "a@b.com", Subject = "Hi", Message = "Hello" };

            var redirect = Assert.IsType<RedirectToActionResult>(sut.Contact(model));

            Assert.Equal("Contact", redirect.ActionName);
            Assert.NotNull(sut.TempData["SuccessMessage"]);
        }

        [Fact]
        public void Error_ReturnsViewWithRequestId()
        {
            var view = Assert.IsType<ViewResult>(CreateSut(TestFactory.CreateContext()).Error());
            var model = Assert.IsType<ErrorViewModel>(view.Model);
            Assert.False(string.IsNullOrEmpty(model.RequestId));
        }
    }
}
