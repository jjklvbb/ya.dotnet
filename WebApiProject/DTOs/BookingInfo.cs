using WebApiProject.Entities;

namespace WebApiProject.DTOs
{
    public record BookingInfo(
        Guid Id,
        Guid EventId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt);
}
