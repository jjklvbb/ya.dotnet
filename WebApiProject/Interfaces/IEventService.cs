using WebApiProject.Entities;
using WebApiProject.DTOs;

namespace WebApiProject.Interfaces
{
    public interface IEventService
    {
        PagedResult<Event> GetEvents(EventFilterParameters filter, int page, int pageSize);

        Event GetEventById(Guid id);

        void CreateEvent(Event newEvent);

        void UpdateEvent(Guid id, string title, string? description, DateTime startAt, DateTime endAt);

        void DeleteEvent(Guid id);
    }
}
