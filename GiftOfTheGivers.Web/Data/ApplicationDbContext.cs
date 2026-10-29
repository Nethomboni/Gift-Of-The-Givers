using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Data
{
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

            builder.Entity<Donation>(entity =>
            {
                entity.HasIndex(d => d.ReferenceNumber).IsUnique();
                entity.Property(d => d.Amount).HasPrecision(18, 2);
            });

            builder.Entity<Project>(entity =>
            {
                entity.Property(p => p.TargetAmount).HasPrecision(18, 2);
                entity.Property(p => p.RaisedAmount).HasPrecision(18, 2);
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