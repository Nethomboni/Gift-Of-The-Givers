using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Data
{
    // Sets up the in-memory database and seeds roles, two demo accounts, and
    // realistic prototype data (Section 39) so the app is screenshot-ready
    // the first time it runs. Every step is idempotent - safe to run on
    // every application startup.
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            await context.Database.EnsureCreatedAsync();

            await SeedRolesAsync(roleManager);
            var donorUser = await SeedUsersAsync(userManager, roleManager, configuration);
            await SeedProjectsAndUpdatesAsync(context);
            await SeedDonationsAsync(context, donorUser);
            await SeedVolunteersAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Employee", "Donor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task<ApplicationUser?> SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            var employeeEmail = configuration["SeedAccounts:EmployeeEmail"] ?? "employee@giftofthegivers.local";
            var employeePassword = configuration["SeedAccounts:EmployeePassword"] ?? "Employee@123";
            var donorEmail = configuration["SeedAccounts:DonorEmail"] ?? "donor@giftofthegivers.local";
            var donorPassword = configuration["SeedAccounts:DonorPassword"] ?? "Donor@123";

            var employee = await userManager.FindByEmailAsync(employeeEmail);
            if (employee == null)
            {
                employee = new ApplicationUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    FullName = "Naledi Mokoena",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(employee, employeePassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employee, "Employee");
                }
            }

            var donor = await userManager.FindByEmailAsync(donorEmail);
            if (donor == null)
            {
                donor = new ApplicationUser
                {
                    UserName = donorEmail,
                    Email = donorEmail,
                    FullName = "Sipho Dlamini",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(donor, donorPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(donor, "Donor");
                }
            }

            return donor;
        }

        private static async Task SeedProjectsAndUpdatesAsync(ApplicationDbContext context)
        {
            if (await context.Projects.AnyAsync())
            {
                return;
            }

            var projects = new List<Project>
            {
                new Project
                {
                    Name = "Limpopo Flood Relief",
                    Location = "Limpopo, South Africa",
                    Description = "Emergency food, clean water, and shelter support for communities displaced by seasonal flooding in Limpopo.",
                    TargetAmount = 500000m,
                    RaisedAmount = 342000m,
                    ImageUrl = "/images/projects/flood-relief.svg",
                    IconClass = "bi-droplet-fill",
                    Status = "Ongoing",
                    Beneficiaries = 1850,
                    CreatedAt = DateTime.Now.AddDays(-64)
                },
                new Project
                {
                    Name = "Gauteng Food Distribution",
                    Location = "Gauteng, South Africa",
                    Description = "Weekly food parcels and hot meals for vulnerable households across the greater Johannesburg and Tshwane metros.",
                    TargetAmount = 250000m,
                    RaisedAmount = 210500m,
                    ImageUrl = "/images/projects/food-distribution.svg",
                    IconClass = "bi-basket2-fill",
                    Status = "Ongoing",
                    Beneficiaries = 3200,
                    CreatedAt = DateTime.Now.AddDays(-51)
                },
                new Project
                {
                    Name = "KwaZulu-Natal Community Support",
                    Location = "KwaZulu-Natal, South Africa",
                    Description = "Rebuilding support, temporary shelter materials, and household essentials for families affected by storm damage.",
                    TargetAmount = 400000m,
                    RaisedAmount = 156000m,
                    ImageUrl = "/images/projects/community-support.svg",
                    IconClass = "bi-people-fill",
                    Status = "Ongoing",
                    Beneficiaries = 980,
                    CreatedAt = DateTime.Now.AddDays(-38)
                },
                new Project
                {
                    Name = "Eastern Cape Medical Outreach",
                    Location = "Eastern Cape, South Africa",
                    Description = "Mobile clinics and basic medical supplies reaching rural communities with limited access to healthcare.",
                    TargetAmount = 300000m,
                    RaisedAmount = 95000m,
                    ImageUrl = "/images/projects/medical-support.svg",
                    IconClass = "bi-heart-pulse-fill",
                    Status = "Ongoing",
                    Beneficiaries = 640,
                    CreatedAt = DateTime.Now.AddDays(-22)
                },
                new Project
                {
                    Name = "Western Cape Education Support",
                    Location = "Western Cape, South Africa",
                    Description = "School supplies, uniforms, and scholar transport support for learners from under-resourced households.",
                    TargetAmount = 150000m,
                    RaisedAmount = 150000m,
                    ImageUrl = "/images/projects/education-support.svg",
                    IconClass = "bi-book-fill",
                    Status = "Completed",
                    Beneficiaries = 500,
                    CreatedAt = DateTime.Now.AddDays(-140)
                },
                new Project
                {
                    Name = "Mpumalanga Winter Blanket Drive",
                    Location = "Mpumalanga, South Africa",
                    Description = "Blankets and warm clothing distributed ahead of the winter cold snap to families in informal settlements.",
                    TargetAmount = 80000m,
                    RaisedAmount = 82400m,
                    ImageUrl = "/images/projects/community-support.svg",
                    IconClass = "bi-cloud-snow-fill",
                    Status = "Completed",
                    Beneficiaries = 1100,
                    CreatedAt = DateTime.Now.AddDays(-200)
                }
            };

            context.Projects.AddRange(projects);
            await context.SaveChangesAsync();

            Project ById(string name) => projects.First(p => p.Name == name);
            const string employeeName = "Naledi Mokoena";

            var updates = new List<ProjectUpdate>
            {
                new ProjectUpdate
                {
                    ProjectId = ById("Limpopo Flood Relief").Id,
                    Title = "Food parcels distributed to affected families",
                    Description = "Our relief team has successfully distributed food parcels to communities affected by flooding.",
                    Location = "Limpopo",
                    PostedByName = employeeName,
                    CreatedAt = DateTime.Now.AddDays(-9)
                },
                new ProjectUpdate
                {
                    ProjectId = ById("Limpopo Flood Relief").Id,
                    Title = "Second relief convoy reaches remote villages",
                    Description = "A follow-up convoy delivered clean water and hygiene kits to villages that were previously cut off by damaged roads.",
                    Location = "Limpopo",
                    PostedByName = employeeName,
                    CreatedAt = DateTime.Now.AddDays(-3)
                },
                new ProjectUpdate
                {
                    ProjectId = ById("Gauteng Food Distribution").Id,
                    Title = "Weekend distribution reaches 3,200 residents",
                    Description = "Volunteers completed the weekend food distribution programme across five community centres.",
                    Location = "Gauteng",
                    PostedByName = employeeName,
                    CreatedAt = DateTime.Now.AddDays(-6)
                },
                new ProjectUpdate
                {
                    ProjectId = ById("KwaZulu-Natal Community Support").Id,
                    Title = "Temporary shelter materials delivered",
                    Description = "Building materials and blankets were delivered to families displaced by recent storm damage.",
                    Location = "KwaZulu-Natal",
                    PostedByName = employeeName,
                    CreatedAt = DateTime.Now.AddDays(-11)
                },
                new ProjectUpdate
                {
                    ProjectId = ById("Eastern Cape Medical Outreach").Id,
                    Title = "Mobile clinic provides check-ups in rural communities",
                    Description = "Medical support teams provided assistance and basic check-ups to vulnerable community members.",
                    Location = "Eastern Cape",
                    PostedByName = employeeName,
                    CreatedAt = DateTime.Now.AddDays(-4)
                },
                new ProjectUpdate
                {
                    ProjectId = ById("Western Cape Education Support").Id,
                    Title = "Final school-supplies handover marks project completion",
                    Description = "The last batch of uniforms and stationery was handed over, bringing this project to a successful close.",
                    Location = "Western Cape",
                    PostedByName = employeeName,
                    CreatedAt = DateTime.Now.AddDays(-40)
                }
            };

            context.ProjectUpdates.AddRange(updates);
            await context.SaveChangesAsync();
        }

        private static async Task SeedDonationsAsync(ApplicationDbContext context, ApplicationUser? donorUser)
        {
            if (await context.Donations.AnyAsync())
            {
                return;
            }

            var donations = new List<Donation>
            {
                new Donation { DonationType = "OneOff", Currency = "ZAR", Amount = 500m, Cause = "General Relief", IsAnonymous = false, DonorName = "Thabo Mahlangu", CreatedAt = DateTime.Now.AddDays(-30) },
                new Donation { DonationType = "Recurring", Frequency = "Monthly", Currency = "ZAR", Amount = 1500m, Cause = "Disaster Relief", IsAnonymous = true, DonorName = "Anonymous Donor", CreatedAt = DateTime.Now.AddDays(-27) },
                new Donation { DonationType = "OneOff", Currency = "USD", Amount = 50m, Cause = "Food Support", IsAnonymous = false, DonorName = "Emma Johnson", CreatedAt = DateTime.Now.AddDays(-24) },
                new Donation { DonationType = "OneOff", Currency = "ZAR", Amount = 250m, Cause = "Medical Aid", IsAnonymous = true, DonorName = "Anonymous Donor", CreatedAt = DateTime.Now.AddDays(-20) },
                new Donation { DonationType = "OneOff", Currency = "EUR", Amount = 100m, Cause = "Education", IsAnonymous = false, DonorName = "Lerato Sithole", CreatedAt = DateTime.Now.AddDays(-17) },
                new Donation { DonationType = "Recurring", Frequency = "Quarterly", Currency = "ZAR", Amount = 5000m, Cause = "General Relief", IsAnonymous = false, DonorName = donorUser?.FullName ?? "Sipho Dlamini", UserId = donorUser?.Id, CreatedAt = DateTime.Now.AddDays(-15) },
                new Donation { DonationType = "OneOff", Currency = "ZAR", Amount = 750m, Cause = "Disaster Relief", IsAnonymous = false, DonorName = donorUser?.FullName ?? "Sipho Dlamini", UserId = donorUser?.Id, CreatedAt = DateTime.Now.AddDays(-8) },
                new Donation { DonationType = "OneOff", Currency = "ZAR", Amount = 100m, Cause = "General Relief", IsAnonymous = true, DonorName = "Anonymous Donor", CreatedAt = DateTime.Now.AddDays(-6) },
                new Donation { DonationType = "OneOff", Currency = "ZAR", Amount = 2000m, Cause = "Food Support", IsAnonymous = false, DonorName = "Priya Naidoo", CreatedAt = DateTime.Now.AddDays(-3) },
                new Donation { DonationType = "OneOff", Currency = "ZAR", Amount = 300m, Cause = "Medical Aid", IsAnonymous = false, DonorName = "Johan van der Merwe", CreatedAt = DateTime.Now.AddDays(-1) }
            };

            context.Donations.AddRange(donations);
            await context.SaveChangesAsync();

            foreach (var donation in donations)
            {
                donation.ReferenceNumber = $"GOTG-{donation.CreatedAt:yyyy}-{donation.Id:D6}";
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedVolunteersAsync(ApplicationDbContext context)
        {
            if (await context.Volunteers.AnyAsync())
            {
                return;
            }

            var volunteers = new List<Volunteer>
            {
                new Volunteer { FullName = "Zanele Khumalo", Skills = "Medical, Logistics", Availability = "Weekends, Emergency Response", Email = "zanele.khumalo@example.com", Status = "Contacted", SubmittedAt = DateTime.Now.AddDays(-26) },
                new Volunteer { FullName = "Michael Chen", Skills = "IT, Social Media", Availability = "Evenings, Flexible", Email = "michael.chen@example.com", Status = "New", SubmittedAt = DateTime.Now.AddDays(-19) },
                new Volunteer { FullName = "Nomvula Dube", Skills = "Food Distribution, Driving", Availability = "Weekdays, Weekends", Email = "nomvula.dube@example.com", Phone = "082 555 0134", Status = "Reviewing", SubmittedAt = DateTime.Now.AddDays(-14) },
                new Volunteer { FullName = "Ahmed Patel", Skills = "Administration, Fundraising", Availability = "Flexible", Email = "ahmed.patel@example.com", Status = "New", SubmittedAt = DateTime.Now.AddDays(-11) },
                new Volunteer { FullName = "Grace Botha", Skills = "Teaching, Social Media", Availability = "Weekends", Email = "grace.botha@example.com", Status = "Contacted", SubmittedAt = DateTime.Now.AddDays(-9) },
                new Volunteer { FullName = "Kabelo Molefe", Skills = "Driving, Logistics", Availability = "Emergency Response, Weekdays", Email = "kabelo.molefe@example.com", Phone = "071 555 0198", Status = "Reviewing", SubmittedAt = DateTime.Now.AddDays(-5) },
                new Volunteer { FullName = "Fatima Ismail", Skills = "Medical, Administration", Availability = "Weekdays", Email = "fatima.ismail@example.com", Status = "New", SubmittedAt = DateTime.Now.AddDays(-2) },
                new Volunteer { FullName = "Werner Nel", Skills = "IT, Fundraising", Availability = "Evenings, Flexible", Email = "werner.nel@example.com", Status = "Contacted", SubmittedAt = DateTime.Now.AddDays(-1) }
            };

            context.Volunteers.AddRange(volunteers);
            await context.SaveChangesAsync();

            foreach (var volunteer in volunteers)
            {
                volunteer.ReferenceNumber = $"VOL-{volunteer.SubmittedAt:yyyy}-{volunteer.Id:D5}";
            }
            await context.SaveChangesAsync();
        }
    }
}
