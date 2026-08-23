using WebApiProject.Entities;

namespace WebApiProject.Interfaces
{
    public interface IBookingService
    {
        Task<BookingInfo> CreateBookingAsync(Guid eventId);
        Task<BookingInfo> GetBookingByIdAsync(Guid bookingId);
    }
}
