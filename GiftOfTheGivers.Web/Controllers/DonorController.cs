using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Web.Controllers
{
    [Authorize(Roles = "Donor")]
    public class DonorController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonorController(IDonationService donationService, UserManager<ApplicationUser> userManager)
        {
            _donationService = donationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var history = await _donationService.GetByUserAsync(currentUser.Id);

            var model = new DonorDashboardViewModel
            {
                FullName = currentUser.FullName,
                DonationCount = history.Count,
                TotalDonated = history.Sum(d => d.Amount),
                LatestDonation = history.FirstOrDefault(),
                DonationHistory = history
            };

            return View(model);
        }
    }
}
