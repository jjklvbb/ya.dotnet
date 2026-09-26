using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WebApiProject.BackgroundServices;
using WebApiProject.DataAccess;
using WebApiProject.Entities;

namespace WebApiProject.Test
{
    public class BookingBackgroundServiceTest
    {
        [Fact]
        public async Task BackgroundService_ProcessesPendingBooking()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            using var serviceProvider = services.BuildServiceProvider();

            Guid bookingId;

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var ev = new Event(
                    Guid.NewGuid(),
                    "Test event",
                    null,
                    DateTime.UtcNow.AddHours(1),
                    DateTime.UtcNow.AddHours(2),
                    1);

                var booking = new Booking(ev.Id);

                context.Events.Add(ev);
                context.Bookings.Add(booking);

                await context.SaveChangesAsync();

                bookingId = booking.Id;
            }

            var scopeFactory = serviceProvider
                .GetRequiredService<IServiceScopeFactory>();

            var service = new BookingBackgroundService(
                scopeFactory,
                NullLogger<BookingBackgroundService>.Instance);

            // Act
            await service.StartAsync(CancellationToken.None);

            await Task.Delay(TimeSpan.FromSeconds(3));

            await service.StopAsync(CancellationToken.None);

            // Assert
            using var assertScope = serviceProvider.CreateScope();

            var assertContext = assertScope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var result = await assertContext.Bookings
                .AsNoTracking()
                .SingleAsync(b => b.Id == bookingId);

            Assert.Equal(BookingStatus.Confirmed, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        [Fact]
        public async Task BackgroundService_EventDoesNotExist_RejectsBooking()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            using var serviceProvider = services.BuildServiceProvider();

            Guid bookingId;

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var booking = new Booking(Guid.NewGuid());

                context.Bookings.Add(booking);

                await context.SaveChangesAsync();

                bookingId = booking.Id;
            }

            var scopeFactory = serviceProvider
                .GetRequiredService<IServiceScopeFactory>();

            var service = new BookingBackgroundService(
                scopeFactory,
                NullLogger<BookingBackgroundService>.Instance);

            // Act
            await service.StartAsync(CancellationToken.None);

            await Task.Delay(TimeSpan.FromSeconds(3));

            await service.StopAsync(CancellationToken.None);

            // Assert
            using var assertScope = serviceProvider.CreateScope();

            var assertContext = assertScope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var result = await assertContext.Bookings
                .AsNoTracking()
                .SingleAsync(b => b.Id == bookingId);

            Assert.Equal(BookingStatus.Rejected, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }
    }
}