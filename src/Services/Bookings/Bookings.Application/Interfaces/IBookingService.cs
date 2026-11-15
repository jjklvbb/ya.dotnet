using Bookings.Application.DTOs;

namespace Bookings.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingInfo> CreateBookingAsync(Guid eventId, Guid userId);
        Task<BookingInfo> GetBookingByIdAsync(Guid bookingId);

        Task CancelBookingAsync(
            Guid bookingId,
            Guid userId,
            bool isAdmin);
    }
}
