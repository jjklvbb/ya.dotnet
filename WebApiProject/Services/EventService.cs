using WebApiProject.Interfaces;
using WebApiProject.Models;

namespace WebApiProject.Services
{
    public class EventService : IEventService
    {
        // можно было использовать Dictionary
        public static List<Event> events {  get; private set; }

        public List<Event> GetAllEvents()
        {
            return events;
        }

        public Event? GetEventById(Guid id)
        {
            return events.Where(e => e.Id == id).FirstOrDefault();
        }

        public void CreateEvent(Event newEvent)
        {
            events.Add(newEvent);
        }

        public void UpdateEvent(Guid id, Event newEvent)
        {
            Event? ev = events.Find(e => e.Id == id);

            if (ev == null)
                throw new Exception("Не найдено событие для обновления");

            ev = newEvent;
        }

        public void DeleteEvent(Guid id)
        {
            events.RemoveAll(e => e.Id == id);
        }
    }
}
