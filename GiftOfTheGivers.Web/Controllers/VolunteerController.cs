using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Web.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly IVolunteerService _volunteerService;

        public VolunteerController(IVolunteerService volunteerService)
        {
            _volunteerService = volunteerService;
        }

        public IActionResult Index()
        {
            return View(new VolunteerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(VolunteerViewModel model)
        {
            // [Required] on a List<string> only checks it isn't null, not that
            // it's non-empty, so "at least one selected" is checked here.
            if (model.SelectedSkills.Count == 0)
            {
                ModelState.AddModelError(nameof(model.SelectedSkills), "Please select at least one skill.");
            }

            if (model.SelectedAvailability.Count == 0)
            {
                ModelState.AddModelError(nameof(model.SelectedAvailability), "Please select your availability.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var volunteer = await _volunteerService.CreateVolunteerAsync(model);

            return RedirectToAction(nameof(Confirmation), new { reference = volunteer.ReferenceNumber });
        }

        public async Task<IActionResult> Confirmation(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return NotFound();
            }

            var volunteer = await _volunteerService.GetByReferenceAsync(reference);
            if (volunteer == null)
            {
                return NotFound();
            }

            return View(volunteer);
        }
    }
}
