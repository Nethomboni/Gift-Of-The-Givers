namespace GiftOfTheGivers.Web.Models
{
    // Display-only aggregation for the Employee Dashboard (Section 15).
    // Built entirely by EmployeeController from the database - not a form.
    public class EmployeeDashboardViewModel
    {
        public decimal TotalDonations { get; set; }
        public int ActiveProjectsCount { get; set; }
        public int CompletedProjectsCount { get; set; }
        public int VolunteerSignupsCount { get; set; }

        public List<Donation> RecentDonations { get; set; } = new();
        public List<Volunteer> RecentVolunteers { get; set; } = new();
        public List<Project> OngoingProjects { get; set; } = new();
        public List<ProjectUpdate> RecentUpdates { get; set; } = new();
    }
}
