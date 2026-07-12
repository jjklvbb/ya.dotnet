using WebApiProject.Models;

namespace WebApiProject.Interfaces
{
    public interface IEventService
    {
        Dictionary<Guid, Event> GetAllEvents();

        Event GetEventById(Guid id);

        void CreateEvent(Event newEvent);

        void UpdateEvent(Guid id, Event newEvent);

        void DeleteEvent(Guid id);
    }
}
