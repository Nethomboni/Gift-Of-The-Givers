using GiftOfTheGivers.Tests.Integration.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace GiftOfTheGivers.Tests.Integration
{
    public class AccountWorkflowTests : IClassFixture<GotgWebFactory>
    {
        private readonly GotgWebFactory _factory;
        private readonly ITestOutputHelper _output;

        public AccountWorkflowTests(GotgWebFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        private static List<KeyValuePair<string, string>> RegisterForm(string email) => WebHelper.Form(
            ("FullName", "Test Donor"), ("Email", email),
            ("Password", "Test@1234"), ("ConfirmPassword", "Test@1234"));

        [Fact]
        public async Task RegisteredDonor_CanLogIn_Donate_AndSeeTheDonationOnTheDashboard()
        {
            // Arrange: register a brand-new donor account
            var email = $"donor-{Guid.NewGuid():N}@test.local";
            var register = await WebHelper.PostFormAsync(WebHelper.NewClient(_factory), "/Account/Register", RegisterForm(email));
            WebHelper.Log(_output, register);
            Assert.Equal(HttpStatusCode.Redirect, register.StatusCode);
            Assert.Equal("/Donor/Dashboard", WebHelper.Where(register));

            // Act 1: log in from a clean browser session
            var client = WebHelper.NewClient(_factory);
            var login = await WebHelper.LoginAsync(client, email, "Test@1234");
            WebHelper.Log(_output, login);
            Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
            Assert.Equal("/Donor/Dashboard", WebHelper.Where(login));

            // Act 2: donate while logged in
            var donate = await WebHelper.PostFormAsync(client, "/Donation", WebHelper.Form(
                ("DonationType", "OneOff"), ("Currency", "ZAR"), ("Amount", "250"),
                ("Cause", "Education"), ("IsAnonymous", "false")));
            WebHelper.Log(_output, donate);
            Assert.Equal(HttpStatusCode.Redirect, donate.StatusCode);
            var reference = Regex.Match(WebHelper.Where(donate), @"reference=(.+)$").Groups[1].Value;

            // Assert: saved against this user
            var userId = _factory.WithDb(db => db.Users.Single(u => u.Email == email).Id);
            var donation = _factory.WithDb(db => db.Donations.Single(d => d.ReferenceNumber == reference));
            Assert.Equal(userId, donation.UserId);
            Assert.Equal("Test Donor", donation.DonorName);

            // Assert: it appears on the donor dashboard
            var dashboard = await client.GetAsync("/Donor/Dashboard");
            WebHelper.Log(_output, dashboard);
            Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
            Assert.Contains(reference, await dashboard.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task LoginWithWrongPassword_Fails_AndTheUserStaysSignedOut()
        {
            // Arrange
            var client = WebHelper.NewClient(_factory);

            // Act
            var login = await WebHelper.LoginAsync(client, WebHelper.DonorEmail, "WrongPassword@999");
            WebHelper.Log(_output, login);

            // Assert: login page again with the error, and protected pages stay blocked
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            Assert.Contains("Invalid login attempt", await login.Content.ReadAsStringAsync());

            var dashboard = await client.GetAsync("/Donor/Dashboard");
            WebHelper.Log(_output, dashboard);
            Assert.Equal(HttpStatusCode.Redirect, dashboard.StatusCode);
            Assert.Contains("/Account/Login", WebHelper.Where(dashboard));
        }

        [Fact]
        public async Task DuplicateEmailRegistration_IsRejected()
        {
            // Arrange
            var email = $"dup-{Guid.NewGuid():N}@test.local";
            var first = await WebHelper.PostFormAsync(WebHelper.NewClient(_factory), "/Account/Register", RegisterForm(email));
            WebHelper.Log(_output, first);
            Assert.Equal(HttpStatusCode.Redirect, first.StatusCode);

            // Act: same email again
            var second = await WebHelper.PostFormAsync(WebHelper.NewClient(_factory), "/Account/Register", RegisterForm(email));
            WebHelper.Log(_output, second);

            // Assert
            Assert.Equal(HttpStatusCode.OK, second.StatusCode);
            Assert.Contains("is already taken", await second.Content.ReadAsStringAsync());
            Assert.Equal(1, _factory.WithDb(db => db.Users.Count(u => u.Email == email)));
        }
    }
}