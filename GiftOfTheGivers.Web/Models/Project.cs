using System.ComponentModel.DataAnnotations.Schema;

namespace GiftOfTheGivers.Web.Models
{
    // A relief project shown on the public Projects page and Home page, and
    // managed (updates posted) from the Employee Dashboard.
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal TargetAmount { get; set; }

        public decimal RaisedAmount { get; set; }

        // Local path under wwwroot/images/projects to a themed illustration -
        // see Section 10/34; prototype uses original on-brand artwork rather
        // than real photography.
        public string ImageUrl { get; set; } = string.Empty;

        // Bootstrap Icon class used on cards/badges for this project's category.
        public string IconClass { get; set; } = "bi-droplet-fill";

        public string Status { get; set; } = "Ongoing"; // Ongoing | Completed

        public int Beneficiaries { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ProjectUpdate> Updates { get; set; } = new List<ProjectUpdate>();

        [NotMapped]
        public int ProgressPercentage =>
            TargetAmount <= 0 ? 0 : (int)Math.Min(100, Math.Round(RaisedAmount / TargetAmount * 100, MidpointRounding.AwayFromZero));
    }
}
