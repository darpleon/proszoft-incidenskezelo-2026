using incidenskezelo_backend.Models;
using incidenskezelo_backend.Models.Incident;
using incidenskezelo_backend.Models.Incident.Changes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace incidenskezelo_backend.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Service> Services => Set<Service>();

        public DbSet<Event> Events => Set<Event>();

        public DbSet<User> Users => Set<User>();

        public DbSet<Incident> Incidents => Set<Incident>();

        public DbSet<IncidentCreation> IncidentCreations => Set<IncidentCreation>();

        public DbSet<IncidentStatusChange> IncidentStatusChanges => Set<IncidentStatusChange>();

        public DbSet<IncidentTitleChange> IncidentTitleChanges => Set<IncidentTitleChange>();

        public DbSet<IncidentDescriptionChange> IncidentDescriptionChanges => Set<IncidentDescriptionChange>();

        public DbSet<IncidentPriorityChange> IncidentPriorityChanges => Set<IncidentPriorityChange>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<Enum>()
                .HaveConversion<string>()
                .HaveMaxLength(50);

            configurationBuilder.Properties<DateTime>()
                .HaveConversion(typeof(AsUtcConverter));

            configurationBuilder.Properties<DateTime>()
                .HaveConversion(typeof(AsUtcConverter));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Service", table =>
                    table.HasCheckConstraint("CK_Service_Kind", InList<ServiceKind>("Kind")));
                entity.Property(service => service.Code).HasMaxLength(50);
                entity.Property(service => service.Name).HasMaxLength(100);
                entity.HasIndex(service => service.Code).IsUnique();
                entity.HasData(
                    new Service { ServiceId = 1, Code = "postgres-main", Name = "Main database", Kind = ServiceKind.Database },
                    new Service { ServiceId = 2, Code = "orders-api", Name = "Orders API", Kind = ServiceKind.Api },
                    new Service { ServiceId = 3, Code = "batch-worker", Name = "Batch worker", Kind = ServiceKind.Worker },
                    new Service { ServiceId = 4, Code = "web-portal", Name = "Web portal", Kind = ServiceKind.WebApp },
                    new Service { ServiceId = 5, Code = "file-ingest", Name = "File ingest", Kind = ServiceKind.Worker },
                    new Service { ServiceId = 6, Code = "payment-gw", Name = "Payment gateway", Kind = ServiceKind.External });
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Event", table =>
                {
                    table.HasCheckConstraint("CK_Event_EventType", InList<EventType>("EventType"));
                    table.HasCheckConstraint("CK_Event_Severity", InList<Severity>("Severity"));
                    table.HasCheckConstraint("CK_Event_PayloadJson", "[PayloadJson] IS NULL OR ISJSON([PayloadJson]) = 1");
                });
                entity.Property(evt => evt.SourceEventId).HasMaxLength(100);
                entity.Property(evt => evt.Summary).HasMaxLength(500);
                entity.Property(evt => evt.OccurredAtUtc).HasPrecision(3);
                entity.Property(evt => evt.ReceivedAtUtc).HasPrecision(3);
                entity.HasIndex(evt => evt.SourceEventId).IsUnique();
                entity.HasOne(evt => evt.Service).WithMany().HasForeignKey(evt => evt.ServiceId).OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("AppUser", table =>
                    table.HasCheckConstraint("CK_AppUser_Role", InList<UserRole>("Role")));
                entity.Property(user => user.Name).HasMaxLength(UserLimits.NameMaxLength);
                entity.Property(user => user.Email).HasMaxLength(UserLimits.EmailMaxLength);
                entity.Property(user => user.PasswordHash).HasMaxLength(100);
                entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(20);
                entity.Property(user => user.CreatedAtUtc).HasPrecision(3);
                entity.HasIndex(user => user.Email).IsUnique();
            });

            var incident = modelBuilder.Entity<Incident>();

            incident.Property(i => i.Title).HasMaxLength(IncidentLimits.TitleMaxLength);
            incident.Property(i => i.Description).HasMaxLength(IncidentLimits.DescriptionMaxLength);

            incident.HasIndex(i => i.PublicId).IsUnique();

            incident.HasOne(i => i.Creation)
                .WithOne(c => c.Incident)
                .HasForeignKey<IncidentCreation>(c => c.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            incident.HasMany(i => i.StatusHistory)
                .WithOne(c => c.Incident)
                .HasForeignKey(c => c.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            var statusChange = modelBuilder.Entity<IncidentStatusChange>();

            statusChange.HasIndex(c => new { c.IncidentId, c.OccurredAtUtc });

            incident.HasMany(i => i.TitleHistory)
                .WithOne(c => c.Incident)
                .HasForeignKey(c => c.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            var titleChange = modelBuilder.Entity<IncidentTitleChange>();

            titleChange.Property(c => c.OldValue).HasMaxLength(IncidentLimits.TitleMaxLength);
            titleChange.Property(c => c.NewValue).HasMaxLength(IncidentLimits.TitleMaxLength);
            titleChange.HasIndex(c => new { c.IncidentId, c.OccurredAtUtc });

            incident.HasMany(i => i.DescriptionHistory)
                .WithOne(c => c.Incident)
                .HasForeignKey(c => c.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            var descriptionChange = modelBuilder.Entity<IncidentDescriptionChange>();

            descriptionChange.Property(c => c.OldValue).HasMaxLength(IncidentLimits.DescriptionMaxLength);
            descriptionChange.Property(c => c.NewValue).HasMaxLength(IncidentLimits.DescriptionMaxLength);
            descriptionChange.HasIndex(c => new { c.IncidentId, c.OccurredAtUtc });

            incident.HasMany(i => i.PriorityHistory)
                .WithOne(c => c.Incident)
                .HasForeignKey(c => c.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            var priorityChange = modelBuilder.Entity<IncidentPriorityChange>();

            priorityChange.HasIndex(c => new { c.IncidentId, c.OccurredAtUtc });
        }

        private sealed class AsUtcConverter() : ValueConverter<DateTime, DateTime>(
            value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

        private static string InList<TEnum>(string column) where TEnum : struct, Enum
        {
            var names = Enum.GetNames<TEnum>();
            var violating = names.Where(name => name.Length > 50).ToList();

            if (violating.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{typeof(TEnum).Name} member names exceed the nvarchar(50) model convention ceiling: " +
                    $"{string.Join(", ", violating)}");
            }

            return $"[{column}] IN ({string.Join(", ", names.Select(name => $"'{name}'"))})";
        }
    }
}
