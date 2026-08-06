using WebApiProject.Models;

namespace WebApiProject.Interfaces
{
    public interface IEventService
    {
        PagedResult<Event> GetEvents(EventFilterParameters filter, int page, int pageSize);

        Event GetEventById(Guid id);

        void CreateEvent(Event newEvent);

        void UpdateEvent(Guid id, Event newEvent);

        void DeleteEvent(Guid id);
    }
}
