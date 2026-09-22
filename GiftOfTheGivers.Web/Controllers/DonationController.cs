using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Web.Controllers
{
    // Part 2, Section A wiring lives in the POST Index action and the
    // GenerateTaxCertificate Azure Function under GiftOfTheGivers.Functions.
    public class DonationController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFunctionsApiClient _functionsApi;

        public DonationController(
            IDonationService donationService,
            UserManager<ApplicationUser> userManager,
            IFunctionsApiClient functionsApi)
        {
            _donationService = donationService;
            _userManager = userManager;
            _functionsApi = functionsApi;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.CurrentUserFullName = currentUser?.FullName;
            }

            return View(new DonationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DonationViewModel model)
        {
            bool isLoggedIn = User.Identity?.IsAuthenticated == true;

            // Recurring donations need a frequency; default rather than reject,
            // since the buttons are populated client-side (Section 20).
            if (model.DonationType == "Recurring" && string.IsNullOrWhiteSpace(model.Frequency))
            {
                model.Frequency = "Monthly";
            }

            // A guest who isn't donating anonymously must give a name. Logged-in
            // donors always have a name available from their account, so this
            // only applies to guests (Section 18/23).
            if (!model.IsAnonymous && !isLoggedIn && string.IsNullOrWhiteSpace(model.DonorName))
            {
                ModelState.AddModelError(nameof(model.DonorName), "Please enter your name, or choose to donate anonymously.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? userId = null;
            string? fullName = null;
            if (isLoggedIn)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                userId = currentUser?.Id;
                fullName = currentUser?.FullName;
            }

            var donation = await _donationService.CreateDonationAsync(model, userId, fullName);

            // Part 2, Section A: the moment the donation form is completed,
            // call the GenerateTaxCertificate Azure Function so a certificate
            // number/validation code is ready by the time the donor views
            // their receipt. Best-effort - see FunctionsApiClient.
            var (certificateNumber, validationCode) = await _functionsApi.GenerateTaxCertificateAsync(donation);
            if (certificateNumber != null && validationCode != null)
            {
                await _donationService.SetCertificateInfoAsync(donation.Id, certificateNumber, validationCode);
            }

            return RedirectToAction(nameof(Confirmation), new { reference = donation.ReferenceNumber });
        }

        public async Task<IActionResult> Confirmation(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return NotFound();
            }

            var donation = await _donationService.GetByReferenceAsync(reference);
            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // Placeholder tax certificate (Section 27) - a professional, printable
        // HTML page rather than a server-generated PDF; see Section 62 which
        // explicitly prioritises reliability over complexity here. The "Print /
        // Save as PDF" button uses the browser's own print-to-PDF, so donors can
        // still save a PDF copy without any extra server-side dependency.
        public async Task<IActionResult> Certificate(string reference, bool download = false)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return NotFound();
            }

            var donation = await _donationService.GetByReferenceAsync(reference);
            if (donation == null)
            {
                return NotFound();
            }

            // "Download" reuses the same printable view and simply triggers the
            // browser's print dialog on load, so the donor can save it as a PDF
            // (Section 27/62 - no server-side PDF generation dependency needed).
            ViewBag.AutoPrint = download;

            return View(donation);
        }
    }
}
