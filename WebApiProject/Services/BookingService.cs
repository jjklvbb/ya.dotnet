using WebApiProject.DTOs;
using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;

namespace WebApiProject.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventService _eventService;

        public BookingService(
            IBookingRepository bookingRepository,
            IEventService eventService)
        {
            _bookingRepository = bookingRepository;
            _eventService = eventService;
        }

        public Task<BookingInfo> CreateBookingAsync(Guid eventId)
        {
            _eventService.GetEventById(eventId);

            var booking = new Booking(eventId);
            _bookingRepository.Add(booking);

            return Task.FromResult(ToBookingInfo(booking));
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
