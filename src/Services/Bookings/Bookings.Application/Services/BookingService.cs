using Bookings.Application.DTOs;
using Bookings.Application.Interfaces;
using Bookings.Application.Ports;
using Bookings.Domain.Entities;
using Bookings.Domain.Exceptions;

namespace Bookings.Application.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;

    public BookingService(IBookingRepository bookings)
    {
        _bookings = bookings;
    }

    public async Task<BookingInfo> CreateBookingAsync(
        Guid eventId,
        Guid userId)
    {
        var activeBookings =
            await _bookings.CountActiveByUserIdAsync(userId);

        if (activeBookings >= 10)
        {
            throw new BookingLimitExceededException(
                "У пользователя уже есть 10 активных бронирований.");
        }

        var booking = new Booking(eventId, userId);

        await _bookings.AddAsync(booking);
        await _bookings.SaveChangesAsync();

        return ToBookingInfo(booking);
    }


    public async Task<BookingInfo> GetBookingByIdAsync(
        Guid bookingId,
        Guid userId,
        bool isAdmin)
    {
        var booking = await _bookings.GetByIdAsync(bookingId)
            ?? throw new NotFoundException(
                $"Бронь по ключу {bookingId} не найдена.");

        if (!isAdmin && booking.UserId != userId)
        {
            throw new ForbiddenException(
                "Нельзя просматривать чужую бронь.");
        }

        return ToBookingInfo(booking);
    }

    public async Task CancelBookingAsync(
        Guid bookingId,
        Guid userId,
        bool isAdmin)
    {
        var booking = await _bookings.GetByIdAsync(bookingId)
            ?? throw new NotFoundException(
                $"Бронь по ключу {bookingId} не найдена.");

        if (!isAdmin && booking.UserId != userId)
        {
            throw new ForbiddenException(
                "Нельзя отменить чужую бронь.");
        }

        booking.Cancel();

        await _bookings.SaveChangesAsync();
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