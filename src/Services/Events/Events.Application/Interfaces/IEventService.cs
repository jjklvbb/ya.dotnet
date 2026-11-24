using Events.Application.DTOs;
using Events.Domain.Entities;

namespace Events.Application.Interfaces;

public interface IEventService
{
    Task<PagedResult<Event>> GetEventsAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize);

    Task<EventResponseDto> GetEventByIdAsync(Guid id);

    Task<IReadOnlyList<EventResponseDto>> GetTopEventsAsync();

    Task<Event> CreateEventAsync(
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats);

    Task UpdateEventAsync(
        Guid id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt);

    Task DeleteEventAsync(Guid id);
}
