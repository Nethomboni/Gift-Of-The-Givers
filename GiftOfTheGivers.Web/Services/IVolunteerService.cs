using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Web.Services
{
    public interface IVolunteerService
    {
        // Saves the sign-up, then generates and stores a reference number
        // (e.g. VOL-2026-00128) based on the new row's Id.
        Task<Volunteer> CreateVolunteerAsync(VolunteerViewModel model);

        Task<Volunteer?> GetByReferenceAsync(string referenceNumber);
        Task<List<Volunteer>> GetAllAsync();
        Task<List<Volunteer>> GetRecentAsync(int count);
        Task<int> GetCountAsync();
    }
}
