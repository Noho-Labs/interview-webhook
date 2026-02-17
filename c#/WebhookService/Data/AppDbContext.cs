using Microsoft.EntityFrameworkCore;
using WebhookService.Models;

namespace WebhookService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<WebhookEvent> WebhookEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasColumnName("id");
            entity.Property(o => o.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
            entity.Property(o => o.UpdatedAt).HasColumnName("updated_at").IsRequired();
        });

        modelBuilder.Entity<WebhookEvent>(entity =>
        {
            entity.ToTable("webhook_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Provider).HasColumnName("provider").HasMaxLength(32).IsRequired();
            entity.Property(e => e.EventId).HasColumnName("event_id").HasMaxLength(128).IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(128).IsRequired();
            entity.Property(e => e.Payload).HasColumnName("payload").IsRequired();
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at").IsRequired();
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.ProcessingError).HasColumnName("processing_error");

            // Mirrors: UniqueConstraint("provider", "event_id", name="uq_provider_event_id")
            entity.HasIndex(e => new { e.Provider, e.EventId })
                  .IsUnique()
                  .HasDatabaseName("uq_provider_event_id");
        });
    }
}
