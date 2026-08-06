using WebApiProject.Interfaces;
using WebApiProject.Models;

namespace WebApiProject.Services
{
    public class EventService : IEventService
    {
        private readonly Dictionary<Guid, Event> _events = new ();

        public Dictionary<Guid, Event> GetAllEvents()
        {
            return _events;
        }

        public Event GetEventById(Guid id)
        {
            return _events[id]; // если не будет элемента - генерируется ошибка KeyNotFountException
        }

        public void CreateEvent(Event newEvent)
        {
            _events.Add(newEvent.Id, newEvent);
        }

        public void UpdateEvent(Guid id, Event newEvent)
        {
            if (_events.ContainsKey(id))
                _events[id] = newEvent;
            else throw new KeyNotFoundException();
        }

        public void DeleteEvent(Guid id)
        {
            if (!_events.Remove(id))
                throw new KeyNotFoundException();
        }
    }
}
