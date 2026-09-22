using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDonationService _donationService;
        private readonly IVolunteerService _volunteerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFunctionsApiClient _functionsApi;

        public EmployeeController(
            ApplicationDbContext context,
            IDonationService donationService,
            IVolunteerService volunteerService,
            UserManager<ApplicationUser> userManager,
            IFunctionsApiClient functionsApi)
        {
            _context = context;
            _donationService = donationService;
            _volunteerService = volunteerService;
            _userManager = userManager;
            _functionsApi = functionsApi;
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.EmployeeName = currentUser?.FullName;

            var model = new EmployeeDashboardViewModel
            {
                TotalDonations = await _donationService.GetTotalAsync(),
                ActiveProjectsCount = await _context.Projects.CountAsync(p => p.Status == "Ongoing"),
                CompletedProjectsCount = await _context.Projects.CountAsync(p => p.Status == "Completed"),
                VolunteerSignupsCount = await _volunteerService.GetCountAsync(),
                RecentDonations = await _donationService.GetRecentAsync(5),
                RecentVolunteers = await _volunteerService.GetRecentAsync(5),
                OngoingProjects = await _context.Projects
                    .Where(p => p.Status == "Ongoing")
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync(),
                RecentUpdates = await _context.ProjectUpdates
                    .Include(u => u.Project)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Volunteers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.EmployeeName = currentUser?.FullName;

            var volunteers = await _volunteerService.GetAllAsync();
            return View(volunteers);
        }

        public async Task<IActionResult> ProjectUpdates()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.EmployeeName = currentUser?.FullName;

            var model = new ProjectUpdateViewModel
            {
                AvailableProjects = await _context.Projects.OrderBy(p => p.Name).ToListAsync()
            };

            ViewBag.RecentUpdates = await _context.ProjectUpdates
                .Include(u => u.Project)
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectUpdates(ProjectUpdateViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.EmployeeName = currentUser?.FullName;

            var project = await _context.Projects.FindAsync(model.ProjectId);
            if (project == null)
            {
                ModelState.AddModelError(nameof(model.ProjectId), "Please choose a valid project.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableProjects = await _context.Projects.OrderBy(p => p.Name).ToListAsync();
                ViewBag.RecentUpdates = await _context.ProjectUpdates
                    .Include(u => u.Project)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .ToListAsync();
                return View(model);
            }

            var update = new ProjectUpdate
            {
                ProjectId = model.ProjectId,
                Title = model.Title,
                Description = model.Description,
                ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl,
                Location = string.IsNullOrWhiteSpace(model.Location) ? project!.Location : model.Location,
                PostedByName = currentUser?.FullName,
                CreatedAt = DateTime.Now
            };

            _context.ProjectUpdates.Add(update);
            await _context.SaveChangesAsync();

            // Part 2, Section A: every posted update is also logged to Azure
            // Table Storage via the LogProjectUpdate Azure Function, giving a
            // durable audit trail independent of the in-memory EF database.
            var logged = await _functionsApi.LogProjectUpdateAsync(update, project!.Name);

            TempData["SuccessMessage"] = logged
                ? $"Update posted to {project!.Name} and logged to Azure Storage."
                : $"Update posted to {project!.Name}. (Azure Storage logging was unavailable - is the Functions host running?)";
            return RedirectToAction(nameof(ProjectUpdates));
        }
    }
}
