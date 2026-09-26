using System.Collections.Concurrent;
using WebApiProject.Entities;
using WebApiProject.Interfaces;

namespace WebApiProject.DataAccess
{
    public class InMemoryEventRepository : IEventRepository
    {
        private readonly ConcurrentDictionary<Guid, Event> _events = new();

        public IEnumerable<Event> GetAll()
        {
            return _events.Values;
        }

        public Event? GetById(Guid id)
        {
            _events.TryGetValue(id, out var result);
            return result;
        }

        public void Add(Event newEvent)
        {
            _events.TryAdd(newEvent.Id, newEvent);
        }

        public void Update(Event newEvent)
        {
            _events[newEvent.Id] = newEvent;
        }

        public bool Delete(Guid id)
        {
            return _events.TryRemove(id, out _);
        }
    }
}