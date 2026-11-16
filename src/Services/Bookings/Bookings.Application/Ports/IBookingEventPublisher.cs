namespace Bookings.Application.Ports;

public interface IBookingEventPublisher
{
    Task PublishConfirmedAsync(
        Guid bookingId,
        Guid eventId,
        Guid userId,
        int seatsCount,
        DateTime confirmedAt,
        CancellationToken cancellationToken = default);
}