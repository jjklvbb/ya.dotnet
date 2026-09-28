using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApiProject.DataAccess;
using WebApiProject.DTOs;
using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;
using WebApiProject.Services;

namespace WebApiProject.Test
{
    public class BookingServiceTest : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Guid _eventId;

        public BookingServiceTest()
        {
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingService, BookingService>();

            _serviceProvider = services.BuildServiceProvider();

            _eventId = Guid.NewGuid();

            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var ev = new Event(
                _eventId,
                "Test event",
                null,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(2),
                10);

            context.Events.Add(ev);
            context.SaveChanges();
        }

        private async Task<Event> CreateTestEventAsync(int totalSeats = 10)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var ev = new Event(
                Guid.NewGuid(),
                "Test event",
                null,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(2),
                totalSeats);

            context.Events.Add(ev);
            await context.SaveChangesAsync();

            return ev;
        }

        private async Task<Event> GetEventAsync(Guid eventId)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            return await context.Events
                .AsNoTracking()
                .SingleAsync(e => e.Id == eventId);
        }

        private async Task<BookingInfo> CreateBookingAsync(Guid eventId)
        {
            using var scope = _serviceProvider.CreateScope();

            var service = scope.ServiceProvider
                .GetRequiredService<IBookingService>();

            return await service.CreateBookingAsync(eventId);
        }

        private async Task<BookingInfo> GetBookingAsync(Guid bookingId)
        {
            using var scope = _serviceProvider.CreateScope();

            var service = scope.ServiceProvider
                .GetRequiredService<IBookingService>();

            return await service.GetBookingByIdAsync(bookingId);
        }

        private async Task ConfirmBookingAsync(Guid bookingId)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var booking = await context.Bookings
                .SingleAsync(b => b.Id == bookingId);

            booking.Confirm();

            await context.SaveChangesAsync();
        }

        private async Task RejectBookingAndReleaseSeatAsync(Guid bookingId)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var booking = await context.Bookings
                .SingleAsync(b => b.Id == bookingId);

            var ev = await context.Events
                .SingleAsync(e => e.Id == booking.EventId);

            booking.Reject();
            ev.ReleaseSeats();

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateBooking_ExistingEvent_ReturnsPendingBooking()
        {
            var result = await CreateBookingAsync(_eventId);

            Assert.Equal(_eventId, result.EventId);
            Assert.Equal(BookingStatus.Pending, result.Status);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Null(result.ProcessedAt);
        }

        [Fact]
        public async Task CreateBooking_SameEventTwice_ReturnsUniqueIds()
        {
            var firstBooking = await CreateBookingAsync(_eventId);
            var secondBooking = await CreateBookingAsync(_eventId);

            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        }

        [Fact]
        public async Task GetBookingById_ExistingBooking_ReturnsBooking()
        {
            var createdBooking = await CreateBookingAsync(_eventId);

            var result = await GetBookingAsync(createdBooking.Id);

            Assert.Equal(createdBooking.Id, result.Id);
            Assert.Equal(createdBooking.EventId, result.EventId);
            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task GetBookingById_AfterConfirm_ReturnsUpdatedStatus()
        {
            var createdBooking = await CreateBookingAsync(_eventId);

            await ConfirmBookingAsync(createdBooking.Id);

            var result = await GetBookingAsync(createdBooking.Id);

            Assert.Equal(BookingStatus.Confirmed, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingById_AfterReject_ReturnsUpdatedStatus()
        {
            var createdBooking = await CreateBookingAsync(_eventId);

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var booking = await context.Bookings
                    .SingleAsync(b => b.Id == createdBooking.Id);

                booking.Reject();

                await context.SaveChangesAsync();
            }

            var result = await GetBookingAsync(createdBooking.Id);

            Assert.Equal(BookingStatus.Rejected, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        [Fact]
        public async Task CreateBooking_NonExistingEvent_ThrowsNotFoundException()
        {
            var eventId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => CreateBookingAsync(eventId));

            Assert.Contains(eventId.ToString(), exception.Message);
        }

        [Fact]
        public async Task CreateBooking_DeletedEvent_ThrowsNotFoundException()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var ev = await context.Events
                    .SingleAsync(e => e.Id == _eventId);

                context.Events.Remove(ev);

                await context.SaveChangesAsync();
            }

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => CreateBookingAsync(_eventId));

            Assert.Contains(_eventId.ToString(), exception.Message);
        }

        [Fact]
        public async Task GetBookingById_NonExistingBooking_ThrowsNotFoundException()
        {
            var bookingId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => GetBookingAsync(bookingId));

            Assert.Contains(bookingId.ToString(), exception.Message);
        }

        [Fact]
        public async Task CreateBooking_ExistingEvent_DecreasesAvailableSeats()
        {
            var ev = await CreateTestEventAsync(3);

            await CreateBookingAsync(ev.Id);

            var updatedEvent = await GetEventAsync(ev.Id);

            Assert.Equal(2, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task CreateBooking_NoAvailableSeats_ThrowsNoAvailableSeatsException()
        {
            var ev = await CreateTestEventAsync(1);

            await CreateBookingAsync(ev.Id);

            await Assert.ThrowsAsync<NoAvailableSeatsException>(
                () => CreateBookingAsync(ev.Id));

            var updatedEvent = await GetEventAsync(ev.Id);

            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task CreateBooking_ConcurrentRequests_PreventsOverbooking()
        {
            var ev = await CreateTestEventAsync(5);

            const int concurrentRequests = 20;

            var tasks = Enumerable.Range(0, concurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();

                    var service = scope.ServiceProvider
                        .GetRequiredService<IBookingService>();

                    try
                    {
                        await service.CreateBookingAsync(ev.Id);
                        return true;
                    }
                    catch (NoAvailableSeatsException)
                    {
                        return false;
                    }
                }))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            Assert.Equal(5, results.Count(result => result));
            Assert.Equal(15, results.Count(result => !result));

            var updatedEvent = await GetEventAsync(ev.Id);

            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task CreateBooking_ConcurrentRequests_CreatesUniqueIds()
        {
            var ev = await CreateTestEventAsync(10);

            const int concurrentRequests = 10;

            var tasks = Enumerable.Range(0, concurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();

                    var service = scope.ServiceProvider
                        .GetRequiredService<IBookingService>();

                    return await service.CreateBookingAsync(ev.Id);
                }))
                .ToArray();

            var bookings = await Task.WhenAll(tasks);

            Assert.Equal(10, bookings.Length);

            Assert.Equal(
                10,
                bookings
                    .Select(booking => booking.Id)
                    .Distinct()
                    .Count());

            var updatedEvent = await GetEventAsync(ev.Id);

            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task RejectBooking_ReleaseSeats_RestoresAvailableSeats()
        {
            var ev = await CreateTestEventAsync(1);

            var booking = await CreateBookingAsync(ev.Id);

            var afterBooking = await GetEventAsync(ev.Id);
            Assert.Equal(0, afterBooking.AvailableSeats);

            await RejectBookingAndReleaseSeatAsync(booking.Id);

            var result = await GetBookingAsync(booking.Id);
            var updatedEvent = await GetEventAsync(ev.Id);

            Assert.Equal(BookingStatus.Rejected, result.Status);
            Assert.Equal(1, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task RejectBooking_ReleaseSeats_AllowsNewBooking()
        {
            var ev = await CreateTestEventAsync(1);

            var firstBooking = await CreateBookingAsync(ev.Id);

            await RejectBookingAndReleaseSeatAsync(firstBooking.Id);

            var secondBooking = await CreateBookingAsync(ev.Id);

            Assert.Equal(BookingStatus.Pending, secondBooking.Status);
            Assert.NotEqual(firstBooking.Id, secondBooking.Id);

            var updatedEvent = await GetEventAsync(ev.Id);

            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }
    }
}