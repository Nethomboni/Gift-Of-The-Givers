using System.Net;
using GiftOfTheGivers.Tests.Integration.Support;
using Xunit.Abstractions;

namespace GiftOfTheGivers.Tests.Integration
{
    public class PublicPagesTests : IClassFixture<GotgWebFactory>
    {
        private readonly GotgWebFactory _factory;
        private readonly ITestOutputHelper _output;

        public PublicPagesTests(GotgWebFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Home/About")]
        [InlineData("/Home/Contact")]
        [InlineData("/Donation")]
        [InlineData("/Projects")]
        [InlineData("/Volunteer")]
        public async Task Get_PublicPages_ReturnSuccess(string url)
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);

            // Act
            var response = await client.GetAsync(url);
            WebHelper.Log(_output, response);

            // Assert
            response.EnsureSuccessStatusCode();   // status code 200-299
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType!.ToString());
        }

        [Fact]
        public async Task ProjectsPage_ListsTheSeededProjects()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);

            // Act
            var response = await client.GetAsync("/Projects");
            WebHelper.Log(_output, response);
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Limpopo Flood Relief", html);
            Assert.Contains("Gauteng Food Distribution", html);
        }

        [Fact]
        public async Task ContactForm_ValidMessage_RedirectsAndShowsThanks()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);
            var form = WebHelper.Form(
                ("Name", "Test Visitor"), ("Email", "visitor@test.local"),
                ("Subject", "Integration test"), ("Message", "Hello from an automated test."));

            // Act
            var post = await WebHelper.PostFormAsync(client, "/Home/Contact", form);
            WebHelper.Log(_output, post);
            Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
            var page = await client.GetAsync(WebHelper.Where(post));
            WebHelper.Log(_output, page);

            // Assert
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);
            Assert.Contains("Thanks for reaching out!", await page.Content.ReadAsStringAsync());
        }
    }
}