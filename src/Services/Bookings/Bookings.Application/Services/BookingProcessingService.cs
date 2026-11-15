using Bookings.Application.Interfaces;
using Bookings.Application.Ports;
using Bookings.Domain.Entities;

namespace Bookings.Application.Services;

public sealed class BookingProcessingService
    : IBookingProcessingService
{
    private readonly IBookingRepository _bookings;
    private readonly IBookingEventPublisher _publisher;

    public BookingProcessingService(
        IBookingRepository bookings,
        IBookingEventPublisher publisher)
    {
        _bookings = bookings;
        _publisher = publisher;
    }

    public async Task ProcessAsync(
        Guid bookingId,
        CancellationToken ct = default)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, ct);

        if (booking is null ||
            booking.Status != BookingStatus.Pending)
        {
            return;
        }

        booking.Confirm();

        await _bookings.SaveChangesAsync(ct);

        await _publisher.PublishConfirmedAsync(
            booking.Id,
            booking.EventId,
            ct);
    }


    public async Task RejectAsync(
        Guid bookingId,
        CancellationToken ct = default)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, ct);

        if (booking is null ||
            booking.Status != BookingStatus.Pending)
        {
            return;
        }

        booking.Reject();

        await _bookings.SaveChangesAsync(ct);
    }
}