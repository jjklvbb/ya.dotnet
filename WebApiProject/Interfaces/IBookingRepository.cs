using WebApiProject.Entities;

namespace WebApiProject.Interfaces
{
    public interface IBookingRepository
    {
        void Add(Booking booking);
        Booking? GetById(Guid id);
        IEnumerable<Booking> GetPending();
        void Update(Booking booking);
    }
}
