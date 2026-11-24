using Events.Domain.Entities;

namespace Events.Application.DTOs;

public sealed record EventResponseDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartAt,
    DateTime EndAt,
    int TotalSeats,
    int AvailableSeats)
{
    public static EventResponseDto FromEntity(Event ev) => new(
        ev.Id,
        ev.Title,
        ev.Description,
        ev.StartAt,
        ev.EndAt,
        ev.TotalSeats,
        ev.AvailableSeats);
}
