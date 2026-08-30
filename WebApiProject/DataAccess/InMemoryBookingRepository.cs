using System.Collections.Concurrent;
using WebApiProject.Entities;
using WebApiProject.Interfaces;

namespace WebApiProject.DataAccess
{
    public class InMemoryBookingRepository : IBookingRepository
    {
        private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

        public void Add(Booking booking)
        {
            _bookings.TryAdd(booking.Id, booking);
        }

        public Booking? GetById(Guid id)
        {
            _bookings.TryGetValue(id, out var booking);
            return booking;
        }

        public IEnumerable<Booking> GetPending()
        {
            return _bookings.Values
                .Where(b => b.Status == BookingStatus.Pending)
                .ToList();
        }

        public void Update(Booking booking)
        {
            _bookings[booking.Id] = booking;
        }
    }
}
