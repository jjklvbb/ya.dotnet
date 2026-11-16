
using Bookings.Application.Ports;
using Bookings.Application.Services;
using Bookings.Domain.Entities;
using Bookings.Domain.Exceptions;

namespace Bookings.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task GetById_Owner_CanReadBooking()
    {
        var ownerId = Guid.NewGuid();
        var booking = new Booking(Guid.NewGuid(), ownerId);
        var service = CreateService(booking);

        var result = await service.GetBookingByIdAsync(
            booking.Id, ownerId, false);

        Assert.Equal(booking.Id, result.Id);
    }

    [Fact]
    public async Task GetById_OtherUser_ThrowsForbiddenException()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid());
        var service = CreateService(booking);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.GetBookingByIdAsync(
                booking.Id, Guid.NewGuid(), false));
    }

    [Fact]
    public async Task GetById_Admin_CanReadOtherUsersBooking()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid());
        var service = CreateService(booking);

        var result = await service.GetBookingByIdAsync(
            booking.Id, Guid.NewGuid(), true);

        Assert.Equal(booking.Id, result.Id);
    }

    [Fact]
    public async Task GetById_UnknownBooking_ThrowsNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetBookingByIdAsync(
                Guid.NewGuid(), Guid.NewGuid(), false));
    }

    private static BookingService CreateService(
        params Booking[] bookings)
    {
        return new BookingService(new FakeBookingRepository(bookings));
    }

    private sealed class FakeBookingRepository : IBookingRepository
    {
        private readonly Dictionary<Guid, Booking> _bookings;

        public FakeBookingRepository(IEnumerable<Booking> bookings)
        {
            _bookings = bookings.ToDictionary(x => x.Id);
        }

        public Task<Booking?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _bookings.TryGetValue(id, out var booking);
            return Task.FromResult(booking);
        }

        public Task<IReadOnlyList<Guid>> GetPendingIdsAsync(
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Guid> ids = _bookings.Values
                .Where(x => x.Status == BookingStatus.Pending)
                .Select(x => x.Id)
                .ToList();

            return Task.FromResult(ids);
        }

        public Task AddAsync(
            Booking booking,
            CancellationToken cancellationToken = default)
        {
            _bookings.Add(booking.Id, booking);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<int> CountActiveByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var count = _bookings.Values.Count(
                x => x.UserId == userId &&
                     (x.Status == BookingStatus.Pending ||
                      x.Status == BookingStatus.Confirmed));

            return Task.FromResult(count);
        }
    }
}
