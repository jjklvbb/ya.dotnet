using WebApiProject.Interfaces;
using WebApiProject.Models;

namespace WebApiProject.Services
{
    public class EventService : IEventService
    {
        public static Dictionary<Guid, Event> events {  get; private set; } = new Dictionary<Guid, Event>();

        public Dictionary<Guid, Event> GetAllEvents()
        {
            return events;
        }

        public Event GetEventById(Guid id)
        {
            return events[id]; // если не будет элемента - генерируется ошибка KeyNotFountException
        }

        public void CreateEvent(Event newEvent)
        {
            newEvent.Validate();
            events.Add(newEvent.Id, newEvent);
        }

        public void UpdateEvent(Guid id, Event newEvent)
        {
            if (id != newEvent.Id)
                throw new ArgumentException("Некорректные входные данные. ID события не совпадает");
            newEvent.Validate();
            if (events.ContainsKey(id))
                events[id] = newEvent;
            else throw new KeyNotFoundException();
        }

        public void DeleteEvent(Guid id)
        {
            if (!events.Remove(id))
                throw new KeyNotFoundException();
        }
    }
}
