using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;
using WebApiProject.DTOs;

namespace WebApiProject.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public PagedResult<Event> GetEvents(EventFilterParameters filter, int page = 1, int pageSize = 10)
        {
            IQueryable<Event> query = _eventRepository.GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                query = query.Where(e => e.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.From.HasValue)
            {
                query = query.Where(e => e.StartAt >= filter.From.Value);
            }

            if (filter.To.HasValue)
            {
                query = query.Where(e => e.EndAt <= filter.To.Value);
            }

            int totalItems = query.Count();

            var items = query
                .OrderByDescending(e => e.StartAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Event>(items, page, items.Count, totalItems);
        }

        public Event GetEventById(Guid id)
        {
            return _eventRepository.GetById(id) ?? throw new NotFoundException($"Событие по ключу {id} не найдено.");
        }

        public void CreateEvent(Event newEvent)
        {
            _eventRepository.Add(newEvent);
        }

        public void UpdateEvent( Guid id, string title, string? description, DateTime startAt, DateTime endAt)
        {
            var existingEvent = _eventRepository.GetById(id)
                ?? throw new NotFoundException(
                    $"Событие по ключу {id} не найдено.");

            existingEvent.Update(
                title,
                description,
                startAt,
                endAt);

            _eventRepository.Update(existingEvent);
        }

        public void DeleteEvent(Guid id)
        {
            if (!_eventRepository.Delete(id))
                throw new NotFoundException($"Событие по ключу {id} не найдено.");
        }
    }
}
