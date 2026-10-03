using incidenskezelo_backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace incidenskezelo_backend.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Service> Services => Set<Service>();

        public DbSet<Event> Events => Set<Event>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Service", table =>
                    table.HasCheckConstraint("CK_Service_Kind", InList<ServiceKind>("Kind")));
                entity.Property(service => service.Code).HasMaxLength(50);
                entity.Property(service => service.Name).HasMaxLength(100);
                entity.Property(service => service.Kind).HasConversion<string>().HasMaxLength(20);
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
                entity.Property(evt => evt.EventType).HasConversion<string>().HasMaxLength(50);
                entity.Property(evt => evt.Severity).HasConversion<string>().HasMaxLength(20);
                entity.Property(evt => evt.Summary).HasMaxLength(500);
                entity.Property(evt => evt.OccurredAtUtc).HasPrecision(3).HasConversion(AsUtc);
                entity.Property(evt => evt.ReceivedAtUtc).HasPrecision(3).HasConversion(AsUtc);
                entity.HasIndex(evt => evt.SourceEventId).IsUnique();
                entity.HasOne(evt => evt.Service).WithMany().HasForeignKey(evt => evt.ServiceId).OnDelete(DeleteBehavior.NoAction);
            });
        }


        private static readonly ValueConverter<DateTime, DateTime> AsUtc =
            new(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));


        private static string InList<TEnum>(string column) where TEnum : struct, Enum
        {
            return $"[{column}] IN ({string.Join(", ", Enum.GetNames<TEnum>().Select(name => $"'{name}'"))})";
        }
    }
}
