namespace GiftOfTheGivers.Web.Models
{
    // Display-only model for the Home page hero/statistics/featured projects.
    public class HomeIndexViewModel
    {
        public int VolunteersCount { get; set; }
        public int ActiveProjectsCount { get; set; }

        // These two aren't tracked anywhere in the prototype's data model, so
        // they're clearly-labelled sample figures rather than real organisation
        // statistics (Section 9) - shown on the page with a "sample data" note.
        public int CommunitiesReachedSample { get; set; } = 32;
        public int MealsDistributedSample { get; set; } = 48500;

        public List<Project> FeaturedProjects { get; set; } = new();
    }
}
