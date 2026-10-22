using WebApiProject.DTOs;
using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;

namespace WebApiProject.Services
{
    public class BookingService : IBookingService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBookingRepository _bookingRepository;

        private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

        public BookingService(
            IEventRepository eventRepository,
            IBookingRepository bookingRepository)
        {
            _eventRepository = eventRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingInfo> CreateBookingAsync(Guid eventId)
        {
            await BookingSemaphore.WaitAsync();

            try
            {
                var ev = await _eventRepository.GetByIdAsync(eventId)
                    ?? throw new NotFoundException(
                        $"Событие по ключу {eventId} не найдено.");

                if (!ev.TryReserveSeats())
                {
                    throw new NoAvailableSeatsException(
                        "No available seats for this event");
                }

                var booking = new Booking(eventId);

                await _bookingRepository.AddAsync(booking);

                await _bookingRepository.SaveChangesAsync();

                return ToBookingInfo(booking);
            }
            finally
            {
                BookingSemaphore.Release();
            }
        }

        public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId)
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