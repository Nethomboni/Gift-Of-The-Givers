using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;
using System.Text;

namespace GiftOfTheGivers.Tests.Integration.Support
{
    public static class WebHelper
    {
        // Seeded by SeedData.cs (appsettings "SeedAccounts" section)
        public const string EmployeeEmail = "employee@giftofthegivers.local";
        public const string EmployeePassword = "Employee@123";
        public const string EmployeeFullName = "Naledi Mokoena";
        public const string DonorEmail = "donor@giftofthegivers.local";
        public const string DonorPassword = "Donor@123";

        // A browser-like client: keeps cookies, does NOT follow redirects (so tests
        // can check the 302 and where it points), and uses https like the real site.
        public static HttpClient NewClient(GotgWebFactory factory) =>
            factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
                BaseAddress = new Uri("https://localhost")
            });

        public static List<KeyValuePair<string, string>> Form(params (string Key, string Value)[] pairs) =>
            pairs.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)).ToList();

        // Redirect address as path + query. MVC redirects are relative
        // ("/Donation/Confirmation?reference=..."), cookie-auth redirects are
        // absolute ("https://localhost/Account/Login?ReturnUrl=..."); both work here.
        public static string Where(HttpResponseMessage response)
        {
            var location = response.Headers.Location;
            if (location == null) return string.Empty;
            return location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString;
        }

        // GET a page and read the hidden anti-forgery token from its HTML form.
        public static async Task<string> GetTokenAsync(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
            Assert.True(match.Success, $"No anti-forgery token found on {url}");
            return match.Groups[1].Value;
        }

        // Fetch the form for its token, then POST it exactly like a browser would.
        public static async Task<HttpResponseMessage> PostFormAsync(
            HttpClient client, string url, List<KeyValuePair<string, string>> fields)
        {
            var token = await GetTokenAsync(client, url);
            fields.Add(new KeyValuePair<string, string>("__RequestVerificationToken", token));
            return await client.PostAsync(url, new FormUrlEncodedContent(fields));
        }

        public static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password) =>
            PostFormAsync(client, "/Account/Login", Form(
                ("Email", email), ("Password", password), ("RememberMe", "false")));

        // Fails with a clear message if the real Azure Functions cannot be reached from the test process.
        public static async Task AssertFunctionsAreRunningAsync(bool checkStorage)
        {
            var baseUrl = Environment.GetEnvironmentVariable("FunctionsApi__BaseUrl") ?? "http://localhost:7071/api/";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            try
            {
                var cert = await http.PostAsync(baseUrl + "certificates/generate",
                    new StringContent("{\"ReferenceNumber\":\"HEALTH-CHECK\"}", Encoding.UTF8, "application/json"));
                Assert.True(cert.IsSuccessStatusCode,
                    $"GenerateTaxCertificate at {baseUrl} answered {(int)cert.StatusCode}, expected 200.");

                if (checkStorage)
                {
                    var log = await http.PostAsync(baseUrl + "projectupdates/log",
                        new StringContent("{\"Title\":\"\"}", Encoding.UTF8, "application/json"));
                    Assert.True((int)log.StatusCode == 400,
                        $"LogProjectUpdate at {baseUrl} answered {(int)log.StatusCode}, expected 400 for an empty title. " +
                        "A 500 means the function cannot reach its storage (is Azurite running, and is StorageConnectionString set?).");
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                Assert.Fail($"Cannot reach the Azure Functions at {baseUrl}: {ex.Message}. Start the Functions project first.");
            }
        }
        // Writes lines such as: POST /Donation returned 302 to /Donation/Confirmation?reference=GOTG-2026-000011
        public static void Log(ITestOutputHelper output, HttpResponseMessage response)
        {
            var request = response.RequestMessage!;
            var where = Where(response);
            var to = where.Length == 0 ? "" : $" to {where}";
            output.WriteLine($"{request.Method} {request.RequestUri!.PathAndQuery} returned {(int)response.StatusCode}{to}");
        }
    }
}