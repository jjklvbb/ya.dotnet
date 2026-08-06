using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;
using WebApiProject.Models;

namespace WebApiProject.Services
{
    public class EventService : IEventService
    {
        private readonly Dictionary<Guid, Event> _events = new ();

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

            var result = query
                .OrderByDescending(c => c.StartAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalPages = (int)Math.Ceiling((double)result.Count / pageSize);

            return new PagedResult<Event>(result, pageSize, totalPages, result.Count);
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
