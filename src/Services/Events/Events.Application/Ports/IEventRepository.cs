using Events.Application.DTOs;
using Events.Domain.Entities;

namespace Events.Application.Ports;

public interface IEventRepository
{
    Task<PagedResult<Event>> GetEventsAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Event?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Event>> GetTopEventsAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Event newEvent,
        CancellationToken cancellationToken = default);

    void Remove(Event eventEntity);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
