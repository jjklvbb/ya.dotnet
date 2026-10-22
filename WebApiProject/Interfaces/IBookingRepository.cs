using WebApiProject.Entities;

namespace WebApiProject.Interfaces
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