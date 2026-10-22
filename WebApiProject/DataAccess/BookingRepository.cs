using Microsoft.EntityFrameworkCore;
using WebApiProject.Entities;
using WebApiProject.Interfaces;

namespace WebApiProject.DataAccess
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Booking?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return _context.Bookings
                .FirstOrDefaultAsync(
                    b => b.Id == id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Bookings
                .AsNoTracking()
                .Where(b => b.Status == BookingStatus.Pending)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            Booking booking,
            CancellationToken cancellationToken = default)
        {
            await _context.Bookings.AddAsync(
                booking,
                cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}