using GiftOfTheGivers.Tests.Unit.TestSupport;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Tests.Unit.Controllers
{
    public class ProjectsControllerTests
    {
        [Fact]
        public async Task Index_MixedStatuses_ListsOngoingProjectsFirst()
        {
            var context = TestFactory.CreateContext();
            context.Projects.Add(new Project { Name = "Finished", Status = "Completed", CreatedAt = DateTime.Now });
            context.Projects.Add(new Project { Name = "Active", Status = "Ongoing", CreatedAt = DateTime.Now.AddDays(-10) });
            await context.SaveChangesAsync();

            var view = Assert.IsType<ViewResult>(await new ProjectsController(context).Index());

            var projects = Assert.IsAssignableFrom<List<Project>>(view.Model);
            Assert.Equal("Active", projects[0].Name);
        }

        [Fact]
        public async Task Details_UnknownId_ReturnsNotFound()
        {
            var sut = new ProjectsController(TestFactory.CreateContext());
            Assert.IsType<NotFoundResult>(await sut.Details(999));
        }

        [Fact]
        public async Task Details_KnownId_ReturnsProjectWithUpdates()
        {
            var context = TestFactory.CreateContext();
            var project = new Project { Name = "Flood Relief" };
            project.Updates.Add(new ProjectUpdate { Title = "Day 1", Description = "Started" });
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var view = Assert.IsType<ViewResult>(await new ProjectsController(context).Details(project.Id));

            var model = Assert.IsType<Project>(view.Model);
            Assert.Single(model.Updates);
        }
    }
}
