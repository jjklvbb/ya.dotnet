using Microsoft.EntityFrameworkCore;
using WebApiProject.DTOs;
using WebApiProject.Entities;
using WebApiProject.Interfaces;

namespace WebApiProject.DataAccess
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Event>> GetEventsAsync(
            string? title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Event> query = _context.Events.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(title))
            {
                var normalizedTitle = title.ToLower();

                query = query.Where(e =>
                    e.Title.ToLower().Contains(normalizedTitle));
            }

            if (from.HasValue)
            {
                query = query.Where(e =>
                    e.StartAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e =>
                    e.EndAt <= to.Value);
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(e => e.StartAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Event>(
                items,
                page,
                items.Count,
                totalItems);
        }

        public Task<Event?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return _context.Events
                .FirstOrDefaultAsync(
                    e => e.Id == id,
                    cancellationToken);
        }

        public async Task AddAsync(
            Event newEvent,
            CancellationToken cancellationToken = default)
        {
            await _context.Events.AddAsync(
                newEvent,
                cancellationToken);
        }

        public void Remove(Event eventEntity)
        {
            _context.Events.Remove(eventEntity);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}