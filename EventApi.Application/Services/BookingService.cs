using EventApi.Application.DTOs;
using EventApi.Domain.Entities;
using EventApi.Domain.Exceptions;
using EventApi.Application.Ports;
using EventApi.Application.Interfaces;

namespace EventApi.Application.Services
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

        public async Task<BookingInfo> CreateBookingAsync(
            Guid eventId,
            Guid userId)
        {
            await BookingSemaphore.WaitAsync();

            try
            {
                var ev = await _eventRepository.GetByIdAsync(eventId)
                    ?? throw new NotFoundException(
                        $"Событие по ключу {eventId} не найдено.");

                if (ev.StartAt <= DateTime.UtcNow)
                {
                    throw new EventAlreadyStartedException("Нельзя забронировать начавшееся или прошедшее мероприятие.");
                }

                var activeBookings =
                    await _bookingRepository.CountActiveByUserIdAsync(userId);

                if (activeBookings >= 10)
                {
                    throw new BookingLimitExceededException(
                        "У пользователя уже есть 10 активных бронирований.");
                }

                if (!ev.TryReserveSeats())
                {
                    throw new NoAvailableSeatsException(
                        "No available seats for this event");
                }

                var booking = new Booking(eventId, userId);

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

        public async Task CancelBookingAsync(
            Guid bookingId,
            Guid userId,
            bool isAdmin)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new NotFoundException(
                    $"Бронь по ключу {bookingId} не найдена.");

            if (!isAdmin && booking.UserId != userId)
            {
                throw new ForbiddenException(
                    "Нельзя отменить чужую бронь.");
            }

            // Метод Cancel() уже проверяет, можно ли отменить бронь.
            booking.Cancel();

            var ev = await _eventRepository.GetByIdAsync(booking.EventId)
                ?? throw new NotFoundException(
                    $"Событие по ключу {booking.EventId} не найдено.");

            ev.ReleaseSeats();

            await _bookingRepository.SaveChangesAsync();
        }
    }
}