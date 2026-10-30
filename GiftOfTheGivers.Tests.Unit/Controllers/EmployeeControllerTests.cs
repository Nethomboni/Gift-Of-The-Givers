using System.Security.Claims;
using GiftOfTheGivers.Tests.Unit.TestSupport;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GiftOfTheGivers.Tests.Unit.Controllers
{
    public class EmployeeControllerTests
    {
        private readonly ApplicationDbContext _context = TestFactory.CreateContext();
        private readonly Mock<IDonationService> _donations = new();
        private readonly Mock<IVolunteerService> _volunteers = new();
        private readonly Mock<IFunctionsApiClient> _functionsApi = new();
        private readonly Mock<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>> _userManager = TestFactory.MockUserManager();
        private readonly EmployeeController _sut;

        public EmployeeControllerTests()
        {
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { FullName = "Staff Member" });
            _sut = new EmployeeController(_context, _donations.Object, _volunteers.Object, _userManager.Object, _functionsApi.Object)
            {
                ControllerContext = TestFactory.ControllerContext(),
                TempData = TestFactory.TempData()
            };
        }

        private async Task<Project> SeedProjectAsync(string status = "Ongoing")
        {
            var project = new Project { Name = "Food Drive", Location = "Soweto", Status = status };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        [Fact]
        public async Task Dashboard_WithData_BuildsSummaryFromServicesAndDatabase()
        {
            await SeedProjectAsync("Ongoing");
            await SeedProjectAsync("Completed");
            _donations.Setup(d => d.GetTotalAsync()).ReturnsAsync(1500m);
            _donations.Setup(d => d.GetRecentAsync(5)).ReturnsAsync(new List<Donation> { new() });
            _volunteers.Setup(v => v.GetCountAsync()).ReturnsAsync(7);
            _volunteers.Setup(v => v.GetRecentAsync(5)).ReturnsAsync(new List<Volunteer> { new() });

            var view = Assert.IsType<ViewResult>(await _sut.Dashboard());

            var model = Assert.IsType<EmployeeDashboardViewModel>(view.Model);
            Assert.Equal(1500m, model.TotalDonations);
            Assert.Equal(1, model.ActiveProjectsCount);
            Assert.Equal(1, model.CompletedProjectsCount);
            Assert.Equal(7, model.VolunteerSignupsCount);
            Assert.Single(model.OngoingProjects);
            Assert.Equal("Staff Member", (string)_sut.ViewBag.EmployeeName);
        }

        [Fact]
        public async Task Volunteers_ReturnsViewWithAllVolunteers()
        {
            _volunteers.Setup(v => v.GetAllAsync()).ReturnsAsync(new List<Volunteer> { new(), new() });

            var view = Assert.IsType<ViewResult>(await _sut.Volunteers());

            Assert.Equal(2, Assert.IsAssignableFrom<List<Volunteer>>(view.Model).Count);
        }

        [Fact]
        public async Task ProjectUpdates_Get_ReturnsFormWithAvailableProjects()
        {
            await SeedProjectAsync();

            var view = Assert.IsType<ViewResult>(await _sut.ProjectUpdates());

            var model = Assert.IsType<ProjectUpdateViewModel>(view.Model);
            Assert.Single(model.AvailableProjects);
        }

        [Fact]
        public async Task ProjectUpdates_Post_UnknownProject_ReturnsViewWithErrorAndSavesNothing()
        {
            var model = new ProjectUpdateViewModel { ProjectId = 999, Title = "T", Description = "D" };

            var view = Assert.IsType<ViewResult>(await _sut.ProjectUpdates(model));

            Assert.True(_sut.ModelState.ContainsKey(nameof(ProjectUpdateViewModel.ProjectId)));
            Assert.Empty(await _context.ProjectUpdates.ToListAsync());
            _functionsApi.Verify(f => f.LogProjectUpdateAsync(It.IsAny<ProjectUpdate>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProjectUpdates_Post_Valid_SavesUpdateLogsToFunctionAndRedirects()
        {
            var project = await SeedProjectAsync();
            _functionsApi.Setup(f => f.LogProjectUpdateAsync(It.IsAny<ProjectUpdate>(), "Food Drive")).ReturnsAsync(true);
            var model = new ProjectUpdateViewModel { ProjectId = project.Id, Title = "Delivered", Description = "100 parcels", Location = "" };

            var redirect = Assert.IsType<RedirectToActionResult>(await _sut.ProjectUpdates(model));

            Assert.Equal("ProjectUpdates", redirect.ActionName);
            var saved = await _context.ProjectUpdates.SingleAsync();
            Assert.Equal("Soweto", saved.Location);          // falls back to the project's location
            Assert.Equal("Staff Member", saved.PostedByName);
            Assert.Contains("logged to Azure Storage", (string)_sut.TempData["SuccessMessage"]!);
            _functionsApi.Verify(f => f.LogProjectUpdateAsync(It.IsAny<ProjectUpdate>(), "Food Drive"), Times.Once);
        }

        [Fact]
        public async Task ProjectUpdates_Post_FunctionUnavailable_StillSavesAndShowsFallbackMessage()
        {
            var project = await SeedProjectAsync();
            _functionsApi.Setup(f => f.LogProjectUpdateAsync(It.IsAny<ProjectUpdate>(), It.IsAny<string>())).ReturnsAsync(false);
            var model = new ProjectUpdateViewModel { ProjectId = project.Id, Title = "T", Description = "D", Location = "Cape Town" };

            await _sut.ProjectUpdates(model);

            Assert.Single(await _context.ProjectUpdates.ToListAsync());
            Assert.Contains("unavailable", (string)_sut.TempData["SuccessMessage"]!);
        }
    }
}
