using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Data
{
    // IdentityDbContext<ApplicationUser> gives us the AspNetUsers / AspNetRoles /
    // AspNetUserRoles tables for authentication, alongside our own prototype
    // tables (Donations, Volunteers, Projects, ProjectUpdates).
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Donation> Donations => Set<Donation>();
        public DbSet<Volunteer> Volunteers => Set<Volunteer>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectUpdate> ProjectUpdates => Set<ProjectUpdate>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Note: no HasColumnType(...) calls here - that's a relational-only
            // concept (Microsoft.EntityFrameworkCore.Relational), which isn't
            // part of the dependency graph now that persistence is in-memory
            // only. Precision on the decimal properties is already exact in
            // .NET regardless, so nothing is lost by leaving it out.
            builder.Entity<Donation>(entity =>
            {
                entity.HasIndex(d => d.ReferenceNumber).IsUnique();
            });

            builder.Entity<Volunteer>(entity =>
            {
                entity.HasIndex(v => v.ReferenceNumber).IsUnique();
            });

            builder.Entity<ProjectUpdate>(entity =>
            {
                entity.HasOne(u => u.Project)
                      .WithMany(p => p.Updates)
                      .HasForeignKey(u => u.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
