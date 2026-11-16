using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Events.Infrastructure.Messaging;

namespace Events.Infrastructure.DataAccess;

public sealed class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<ProcessedBookingMessage> ProcessedBookingMessages => Set<ProcessedBookingMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(EventsDbContext).Assembly);

        modelBuilder.Entity<ProcessedBookingMessage>(entity =>
        {
            entity.ToTable("processed_booking_messages");
            entity.HasKey(x => x.BookingId);
            entity.Property(x => x.ProcessedAt).IsRequired();
        });
    }
}