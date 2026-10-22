using WebApiProject.DTOs;
using EventApi.Application.DTOs;
using EventApi.Domain.Entities;

public interface IEventService
{
    Task<PagedResult<Event>> GetEventsAsync(
        EventFilterParameters filter,
        int page,
        int pageSize);

    Task<Event> GetEventByIdAsync(Guid id);

    Task CreateEventAsync(Event newEvent);

    Task UpdateEventAsync(
        Guid id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt);

    Task DeleteEventAsync(Guid id);
}