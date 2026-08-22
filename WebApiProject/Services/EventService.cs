using WebApiProject.Exceptions;
using WebApiProject.Interfaces;
using WebApiProject.Models;

namespace WebApiProject.Services
{
    public class EventService : IEventService
    {
        private readonly Dictionary<Guid, Event> _events = new ();

        public EventService()
        {

        }
        public EventService (List<Event>? list)
        {
            if (list == null)
            {
                _events = new Dictionary<Guid, Event> ();
            }
            else
            {
                foreach (var item in list)
                {
                    _events.Add(item.Id, item);
                }
            }
        }

        public PagedResult<Event> GetEvents(EventFilterParameters filter, int page = 1, int pageSize = 10)
        {
            IQueryable<Event> query = _events.Values.AsQueryable();

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
            try
            {
                return _events[id]; // если не будет элемента - генерируется ошибка KeyNotFountException
            }
            catch(KeyNotFoundException)
            {
                throw new NotFoundException($"Событие по ключу {id} не найдено.");
            }
        }

        public void CreateEvent(Event newEvent)
        {
            _events.Add(newEvent.Id, newEvent);
        }

        public void UpdateEvent(Guid id, Event newEvent)
        {
            if (_events.ContainsKey(id))
                _events[id] = newEvent;
            else throw new NotFoundException($"Событие по ключу {id} не найдено.");
        }

        public void DeleteEvent(Guid id)
        {
            if (!_events.Remove(id))
                throw new NotFoundException($"Событие по ключу {id} не найдено.");
        }
    }
}
