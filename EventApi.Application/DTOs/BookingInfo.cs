using EventApi.Domain.Entities;

namespace EventApi.Application.DTOs
{
    public record BookingInfo(
        Guid Id,
        Guid EventId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt);
}
