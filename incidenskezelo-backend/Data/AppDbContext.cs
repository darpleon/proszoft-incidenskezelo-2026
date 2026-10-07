using incidenskezelo_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace incidenskezelo_backend.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Incident> Incidents => Set<Incident>();

        public DbSet<IncidentStatusChange> IncidentStatusChanges => Set<IncidentStatusChange>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var incident = modelBuilder.Entity<Incident>();

            incident.Property(i => i.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
            incident.Property(i => i.Priority)
                .HasConversion<string>()
                .HasMaxLength(10);

            incident.Property(i => i.Title).HasMaxLength(200);
            incident.Property(i => i.Description).HasMaxLength(4000);

            incident.HasMany(i => i.StatusHistory)
                .WithOne(c => c.Incident)
                .HasForeignKey(c => c.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            var statusChange = modelBuilder.Entity<IncidentStatusChange>();

            statusChange.Property(c => c.FromStatus)
                .HasConversion<string>()
                .HasMaxLength(20);
            statusChange.Property(c => c.ToStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            statusChange.HasIndex(c => new { c.IncidentId, c.OccurredAtUtc });
        }
    }
}
