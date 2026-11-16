namespace Events.Infrastructure.Messaging;

public sealed class ProcessedBookingMessage
{
    public Guid BookingId { get; set; }
    public DateTime ProcessedAt { get; set; }
}