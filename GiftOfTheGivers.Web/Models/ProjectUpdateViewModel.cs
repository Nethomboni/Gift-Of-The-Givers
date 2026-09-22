using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models
{
    // Bound from the "Post an Update" form on the Employee > Project Updates page.
    public class ProjectUpdateViewModel
    {
        [Required(ErrorMessage = "Please choose a project.")]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Please enter an update title.")]
        [StringLength(150)]
        [Display(Name = "Update Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a description.")]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Image URL (optional)")]
        public string? ImageUrl { get; set; }

        [StringLength(150)]
        public string? Location { get; set; }

        // Populates the Project dropdown - set by the controller before rendering.
        public List<Project> AvailableProjects { get; set; } = new();
    }
}
