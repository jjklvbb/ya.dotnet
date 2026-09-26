using WebApiProject.DTOs;
using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;

namespace WebApiProject.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly object _bookingLock = new();

        public BookingService(
            IBookingRepository bookingRepository,
            IEventRepository eventRepository)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
        }

        public Task<BookingInfo> CreateBookingAsync(Guid eventId)
        {
            lock (_bookingLock)
            {
                var ev = _eventRepository.GetById(eventId)
                    ?? throw new NotFoundException(
                        $"Событие по ключу {eventId} не найдено.");

                if (!ev.TryReserveSeats())
                {
                    throw new NoAvailableSeatsException(
                        "No available seats for this event.");
                }

                _eventRepository.Update(ev);

                var booking = new Booking(eventId);
                _bookingRepository.Add(booking);

                return Task.FromResult(ToBookingInfo(booking));
            }
        }

        public Task<BookingInfo> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = _bookingRepository.GetById(bookingId)
                ?? throw new NotFoundException(
                    $"Бронь по ключу {bookingId} не найдена.");

            return Task.FromResult(ToBookingInfo(booking));
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
