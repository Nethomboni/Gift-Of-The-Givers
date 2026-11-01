using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using GiftOfTheGivers.Tests.Integration.Support;
using Xunit.Abstractions;

namespace GiftOfTheGivers.Tests.Integration
{
    // The Employee Dashboard page is deliberately not requested here: it SUMs a
    // decimal column, which SQLite cannot do. The pages tested below do not.
    public class EmployeeWorkflowTests : IClassFixture<GotgWebFactory>
    {
        private readonly GotgWebFactory _factory;
        private readonly ITestOutputHelper _output;

        public EmployeeWorkflowTests(GotgWebFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        private async Task<(HttpClient Client, int ProjectId, string Title)> LoginAndPostUpdateAsync()
        {
            var client = WebHelper.NewClient(_factory);
            var login = await WebHelper.LoginAsync(client, WebHelper.EmployeeEmail, WebHelper.EmployeePassword);
            WebHelper.Log(_output, login);
            Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
            Assert.Equal("/Employee/Dashboard", WebHelper.Where(login));

            var projectId = _factory.WithDb(db => db.Projects.First(p => p.Name == "Limpopo Flood Relief").Id);
            var title = $"Integration test update {Guid.NewGuid():N}";

            var post = await WebHelper.PostFormAsync(client, "/Employee/ProjectUpdates", WebHelper.Form(
                ("ProjectId", projectId.ToString()), ("Title", title),
                ("Description", "Posted by an automated integration test."),
                ("Location", ""), ("ImageUrl", "")));
            WebHelper.Log(_output, post);
            Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
            Assert.Equal("/Employee/ProjectUpdates", WebHelper.Where(post));

            return (client, projectId, title);
        }

        [Fact]
        public async Task Employee_PostsProjectUpdate_AndItAppearsOnThePublicProjectPage()
        {
            // Arrange + Act: log in as the seeded employee and post an update
            var (_, projectId, title) = await LoginAndPostUpdateAsync();

            // Assert: saved with the employee's name
            var saved = _factory.WithDb(db => db.ProjectUpdates.Single(u => u.Title == title));
            Assert.Equal(projectId, saved.ProjectId);
            Assert.Equal(WebHelper.EmployeeFullName, saved.PostedByName);

            // Assert: a visitor who is not logged in can see it on the project page
            var visitor = WebHelper.NewClient(_factory);
            var page = await visitor.GetAsync($"/Projects/Details/{projectId}");
            WebHelper.Log(_output, page);
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);
            Assert.Contains(title, await page.Content.ReadAsStringAsync());
        }

        // Needs the real Azure Function and its storage (Azurite locally, or the deployed app).
        
        [Fact]
        [Trait("Category", "Functions")]
        public async Task Employee_PostedUpdate_IsLoggedToAzureStorageByTheRealFunction()
        {
            // Arrange: the real functions (and their storage) must be reachable
            await WebHelper.AssertFunctionsAreRunningAsync(checkStorage: true);

            // Act
            var (client, _, _) = await LoginAndPostUpdateAsync();

            // Assert: this banner is only worded this way when LogProjectUpdate answered successfully
            var page = await client.GetAsync("/Employee/ProjectUpdates");
            WebHelper.Log(_output, page);
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);
            Assert.Contains("logged to Azure Storage", await page.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task AnonymousUser_IsRedirectedToLogin_FromEmployeePages()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);

            // Act
            var response = await client.GetAsync("/Employee/ProjectUpdates");
            WebHelper.Log(_output, response);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", WebHelper.Where(response));
        }

        [Fact]
        public async Task Donor_IsBlocked_FromEmployeePages()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);
            await WebHelper.LoginAsync(client, WebHelper.DonorEmail, WebHelper.DonorPassword);

            // Act
            var response = await client.GetAsync("/Employee/ProjectUpdates");
            WebHelper.Log(_output, response);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/AccessDenied", WebHelper.Where(response));
        }
    }
}