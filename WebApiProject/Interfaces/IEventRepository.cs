using WebApiProject.DTOs;
using WebApiProject.Entities;

namespace WebApiProject.Interfaces
{
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

        Task AddAsync(
            Event newEvent,
            CancellationToken cancellationToken = default);

        void Remove(Event eventEntity);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}