namespace GiftOfTheGivers.Web.Models
{
    // A field update posted by an Employee against a Project (Section 14/16).
    // Shown on the public Project Details page and in the Employee dashboard.
    public class ProjectUpdate
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public Project? Project { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? PostedByName { get; set; }
    }
}
