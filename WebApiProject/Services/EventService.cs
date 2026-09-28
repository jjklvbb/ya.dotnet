using Microsoft.EntityFrameworkCore;
using WebApiProject.DataAccess;
using WebApiProject.DTOs;
using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;

namespace WebApiProject.Services
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Event>> GetEventsAsync(EventFilterParameters filter, int page = 1, int pageSize = 10)
        {
            IQueryable<Event> query = _context.Events.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                var title = filter.Title.ToLower();

                query = query.Where(e => e.Title.ToLower().Contains(title));
            }

            if (filter.From.HasValue)
            {
                query = query.Where(e => e.StartAt >= filter.From.Value);
            }

            if (filter.To.HasValue)
            {
                query = query.Where(e => e.EndAt <= filter.To.Value);
            }

            int totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.StartAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Event>(
                items,
                page,
                items.Count,
                totalItems);
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            return await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id)
                ?? throw new NotFoundException(
                    $"Событие по ключу {id} не найдено.");
        }

        public async Task CreateEventAsync(Event newEvent)
        {
            _context.Events.Add(newEvent);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateEventAsync(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
        {
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id)
                ?? throw new NotFoundException(
                    $"Событие по ключу {id} не найдено.");

            existingEvent.Update(
                title,
                description,
                startAt,
                endAt);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(Guid id)
        {
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id)
                ?? throw new NotFoundException(
                    $"Событие по ключу {id} не найдено.");

            _context.Events.Remove(existingEvent);

            await _context.SaveChangesAsync();
        }
    }
}
