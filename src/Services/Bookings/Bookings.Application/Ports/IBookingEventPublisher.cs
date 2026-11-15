namespace Bookings.Application.Ports;

public interface IBookingEventPublisher
{
    Task PublishConfirmedAsync(
        Guid bookingId,
        Guid eventId,
        CancellationToken cancellationToken = default);
}