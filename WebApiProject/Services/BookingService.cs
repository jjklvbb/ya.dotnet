using Microsoft.EntityFrameworkCore;
using WebApiProject.DataAccess;
using WebApiProject.DTOs;
using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;

namespace WebApiProject.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BookingInfo> CreateBookingAsync(Guid eventId)
        {
            await BookingSemaphore.WaitAsync();

            try
            {
                var ev = await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == eventId)
                    ?? throw new NotFoundException(
                        $"Событие по ключу {eventId} не найдено.");

                if (!ev.TryReserveSeats())
                {
                    throw new NoAvailableSeatsException(
                        "No available seats for this event");
                }

                var booking = new Booking(eventId);

                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();

                return ToBookingInfo(booking);
            }
            finally
            {
                BookingSemaphore.Release();
            }
        }

        public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new NotFoundException(
                    $"Бронь по ключу {bookingId} не найдена.");

            return ToBookingInfo(booking);
        }

        private static BookingInfo ToBookingInfo(Booking booking)
        {
            return new BookingInfo(
                booking.Id,
                booking.EventId,
                booking.Status,
                booking.CreatedAt,
                booking.ProcessedAt);
        }
    }
}