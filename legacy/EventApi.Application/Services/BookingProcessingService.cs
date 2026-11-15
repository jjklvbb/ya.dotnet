using EventApi.Application.Interfaces;
using EventApi.Application.Ports;
using EventApi.Domain.Entities;

namespace EventApi.Application.Services;

public class BookingProcessingService : IBookingProcessingService
{
    private readonly IBookingRepository _bookings;
    private readonly IEventRepository _events;

    public BookingProcessingService(
        IBookingRepository bookings,
        IEventRepository events)
    {
        _bookings = bookings;
        _events = events;
    }

    public async Task ProcessAsync(
        Guid bookingId,
        CancellationToken ct = default)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, ct);

        if (booking is null || booking.Status != BookingStatus.Pending)
            return;

        var ev = await _events.GetByIdAsync(booking.EventId, ct);

        if (ev is null)
            booking.Reject();
        else
            booking.Confirm();

        await _bookings.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(
        Guid bookingId,
        CancellationToken ct = default)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, ct);

        if (booking is null || booking.Status != BookingStatus.Pending)
            return;

        booking.Reject();

        var ev = await _events.GetByIdAsync(booking.EventId, ct);

        ev?.ReleaseSeats();

        await _bookings.SaveChangesAsync(ct);
    }
}