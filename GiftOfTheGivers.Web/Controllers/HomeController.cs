using System.Diagnostics;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVolunteerService _volunteerService;

        public HomeController(ApplicationDbContext context, IVolunteerService volunteerService)
        {
            _context = context;
            _volunteerService = volunteerService;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProjects = await _context.Projects
                .Where(p => p.Status == "Ongoing")
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();

            var model = new HomeIndexViewModel
            {
                VolunteersCount = await _volunteerService.GetCountAsync(),
                ActiveProjectsCount = await _context.Projects.CountAsync(p => p.Status == "Ongoing"),
                FeaturedProjects = featuredProjects
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Prototype only - no real email service is wired up (Section 36).
            TempData["SuccessMessage"] = "Thanks for reaching out! Our team will get back to you soon.";
            return RedirectToAction(nameof(Contact));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
