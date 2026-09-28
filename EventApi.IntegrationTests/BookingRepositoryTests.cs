using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using WebApiProject.DataAccess;
using WebApiProject.Entities;

namespace EventApi.IntegrationTests;

[Collection(DatabaseCollection.Name)]
public class BookingRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    private static readonly DateTime BaseDate =
        new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public BookingRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        return _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddAsync_AndSaveChangesAsync_PersistsBooking()
    {
        // Arrange
        var eventEntity = CreateEvent();
        await SeedEventAsync(eventEntity);

        var booking = new Booking(eventEntity.Id);

        await using (var context = _fixture.CreateContext())
        {
            var repository = new BookingRepository(context);

            // Act
            await repository.AddAsync(booking);
            await repository.SaveChangesAsync();
        }

        // Assert
        await using var verificationContext = _fixture.CreateContext();

        var actual = await verificationContext.Bookings
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(booking.Id, actual.Id);
        Assert.Equal(eventEntity.Id, actual.EventId);
        Assert.Equal(BookingStatus.Pending, actual.Status);
        Assert.NotEqual(default, actual.CreatedAt);
        Assert.Null(actual.ProcessedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsBooking()
    {
        // Arrange
        var eventEntity = CreateEvent();
        var booking = new Booking(eventEntity.Id);

        await SeedAsync(eventEntity, booking);

        await using var context = _fixture.CreateContext();
        var repository = new BookingRepository(context);

        // Act
        var actual = await repository.GetByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(booking.Id, actual.Id);
        Assert.Equal(eventEntity.Id, actual.EventId);
        Assert.Equal(BookingStatus.Pending, actual.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenBookingDoesNotExist()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new BookingRepository(context);

        // Act
        var actual = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task GetPendingIdsAsync_ReturnsOnlyPendingBookings()
    {
        // Arrange
        var eventEntity = CreateEvent();

        var pending = new Booking(eventEntity.Id);

        var confirmed = new Booking(eventEntity.Id);
        confirmed.Confirm();

        var rejected = new Booking(eventEntity.Id);
        rejected.Reject();

        await SeedAsync(
            eventEntity,
            pending,
            confirmed,
            rejected);

        await using var context = _fixture.CreateContext();
        var repository = new BookingRepository(context);

        // Act
        var ids = await repository.GetPendingIdsAsync();

        // Assert
        Assert.Single(ids);
        Assert.Contains(pending.Id, ids);
        Assert.DoesNotContain(confirmed.Id, ids);
        Assert.DoesNotContain(rejected.Id, ids);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsTrackedBookingChanges()
    {
        // Arrange
        var eventEntity = CreateEvent();
        var booking = new Booking(eventEntity.Id);

        await SeedAsync(eventEntity, booking);

        await using (var context = _fixture.CreateContext())
        {
            var repository = new BookingRepository(context);

            var trackedBooking =
                await repository.GetByIdAsync(booking.Id);

            Assert.NotNull(trackedBooking);

            // Act
            trackedBooking.Confirm();
            await repository.SaveChangesAsync();
        }

        // Assert
        await using var verificationContext = _fixture.CreateContext();

        var actual = await verificationContext.Bookings
            .AsNoTracking()
            .SingleAsync(b => b.Id == booking.Id);

        Assert.Equal(BookingStatus.Confirmed, actual.Status);
        Assert.NotNull(actual.ProcessedAt);
    }

    private static Event CreateEvent()
    {
        return new Event(
            Guid.NewGuid(),
            "Test event",
            "Integration test event",
            BaseDate,
            BaseDate.AddHours(2),
            10);
    }

    private async Task SeedEventAsync(Event eventEntity)
    {
        await using var context = _fixture.CreateContext();

        await context.Events.AddAsync(eventEntity);
        await context.SaveChangesAsync();
    }

    private async Task SeedAsync(
        Event eventEntity,
        params Booking[] bookings)
    {
        await using var context = _fixture.CreateContext();

        await context.Events.AddAsync(eventEntity);
        await context.Bookings.AddRangeAsync(bookings);

        await context.SaveChangesAsync();
    }
}