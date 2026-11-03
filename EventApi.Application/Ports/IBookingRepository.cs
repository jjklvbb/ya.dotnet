using EventApi.Domain.Entities;

namespace EventApi.Application.Ports
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Booking booking,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}