namespace EventApi.Application.Interfaces;

public interface IBookingProcessingService
{
    Task ProcessAsync(Guid bookingId, CancellationToken ct = default);
    Task RejectAsync(Guid bookingId, CancellationToken ct = default);
}