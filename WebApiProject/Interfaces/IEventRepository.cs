using WebApiProject.Entities;

namespace WebApiProject.Interfaces
{
    public interface IEventRepository
    {
        IEnumerable<Event> GetAll();
        Event? GetById(Guid id);
        void Add(Event newEvent);
        void Update(Event newEvent);
        bool Delete(Guid id);
    }
}