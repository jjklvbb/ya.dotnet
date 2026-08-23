namespace WebApiProject.Entities
{
    public record BookingInfo(
        Guid Id,
        Guid EventId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt);
}
