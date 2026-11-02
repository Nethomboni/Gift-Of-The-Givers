using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using GiftOfTheGivers.Tests.Integration.Support;
using Xunit.Abstractions;

namespace GiftOfTheGivers.Tests.Integration
{
    public class VolunteerWorkflowTests : IClassFixture<GotgWebFactory>
    {
        private readonly GotgWebFactory _factory;
        private readonly ITestOutputHelper _output;

        public VolunteerWorkflowTests(GotgWebFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task ValidVolunteerRegistration_IsSaved_AndConfirmationPageShowsReference()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);
            var name = $"Test Volunteer {Guid.NewGuid():N}";
            var form = WebHelper.Form(
                ("FullName", name), ("Email", "volunteer@test.local"), ("Phone", ""),
                ("SelectedSkills", "IT"), ("SelectedSkills", "Driving"),
                ("SelectedAvailability", "Weekends"));

            // Act
            var post = await WebHelper.PostFormAsync(client, "/Volunteer", form);
            WebHelper.Log(_output, post);

            // Assert: redirect and database row
            Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
            var where = WebHelper.Where(post);
            Assert.Matches(@"^/Volunteer/Confirmation\?reference=VOL-\d{4}-\d{5}$", where);

            var reference = Regex.Match(where, @"reference=(.+)$").Groups[1].Value;
            var saved = _factory.WithDb(db => db.Volunteers.Single(v => v.ReferenceNumber == reference));
            Assert.Equal(name, saved.FullName);
            Assert.Equal("IT, Driving", saved.Skills);
            Assert.Equal("Weekends", saved.Availability);
            Assert.Equal("New", saved.Status);

            // Assert: confirmation page
            var confirmation = await client.GetAsync(where);
            WebHelper.Log(_output, confirmation);
            Assert.Equal(HttpStatusCode.OK, confirmation.StatusCode);
            Assert.Contains(reference, await confirmation.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task VolunteerWithNoSkills_IsRejected_AndNothingIsSaved()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);
            var before = _factory.WithDb(db => db.Volunteers.Count());
            var form = WebHelper.Form(
                ("FullName", "No Skills Person"), ("SelectedAvailability", "Weekends"));

            // Act
            var response = await WebHelper.PostFormAsync(client, "/Volunteer", form);
            WebHelper.Log(_output, response);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Please select at least one skill.", await response.Content.ReadAsStringAsync());
            Assert.Equal(before, _factory.WithDb(db => db.Volunteers.Count()));
        }
    }
}