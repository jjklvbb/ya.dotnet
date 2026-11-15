using Bookings.Domain.Entities;

namespace Bookings.Application.DTOs
{
    public record BookingInfo(
        Guid Id,
        Guid EventId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt);
}
