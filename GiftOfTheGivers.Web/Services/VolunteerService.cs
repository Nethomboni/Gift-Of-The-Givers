using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly ApplicationDbContext _context;

        public VolunteerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Volunteer> CreateVolunteerAsync(VolunteerViewModel model)
        {
            var volunteer = new Volunteer
            {
                FullName = model.FullName.Trim(),
                Skills = string.Join(", ", model.SelectedSkills),
                Availability = string.Join(", ", model.SelectedAvailability),
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                SubmittedAt = DateTime.Now,
                Status = "New"
            };

            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();

            volunteer.ReferenceNumber = $"VOL-{volunteer.SubmittedAt:yyyy}-{volunteer.Id:D5}";
            await _context.SaveChangesAsync();

            return volunteer;
        }

        public async Task<Volunteer?> GetByReferenceAsync(string referenceNumber)
        {
            return await _context.Volunteers
                .FirstOrDefaultAsync(v => v.ReferenceNumber == referenceNumber);
        }

        public async Task<List<Volunteer>> GetAllAsync()
        {
            return await _context.Volunteers
                .OrderByDescending(v => v.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Volunteer>> GetRecentAsync(int count)
        {
            return await _context.Volunteers
                .OrderByDescending(v => v.SubmittedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Volunteers.CountAsync();
        }
    }
}
