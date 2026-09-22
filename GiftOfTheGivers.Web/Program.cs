using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Prototype-stage persistence: everything lives in memory for the lifetime of
// the running process - no database to provision, no connection string, no
// migrations (Section 2/32/54 - "dummy storage" is explicitly fine at this
// stage). Data resets on every app restart, which is expected here.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("GiftOfTheGiversDb"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Default Identity password rules - kept intact rather than relaxed,
        // since the seeded demo passwords already satisfy them (Section 61).
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();

// Part 2, Section A: typed HttpClient for the Azure Functions project
// (GiftOfTheGivers.Functions). BaseUrl defaults to the local Functions host
// started via `func start` / F5 in Visual Studio; override
// "FunctionsApi:BaseUrl" in appsettings/App Service configuration once the
// Function App is deployed to Azure.
builder.Services.AddHttpClient<IFunctionsApiClient, FunctionsApiClient>(client =>
{
    var baseUrl = builder.Configuration["FunctionsApi:BaseUrl"] ?? "http://localhost:7071/api/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();
