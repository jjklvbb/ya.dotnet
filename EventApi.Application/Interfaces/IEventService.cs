using EventApi.Application.DTOs;
using EventApi.Domain.Entities;

namespace EventApi.Application.Interfaces;

public interface IEventService
{
    Task<PagedResult<Event>> GetEventsAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize);

    Task<Event> GetEventByIdAsync(Guid id);

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