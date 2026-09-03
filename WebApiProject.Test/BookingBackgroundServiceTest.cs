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
            var bookingRepository = new InMemoryBookingRepository();
            var eventRepository = new InMemoryEventRepository();

            var ev = new Event(
                Guid.NewGuid(),
                "Test event",
                null,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(2),
                1);

            eventRepository.Add(ev);

            var booking = new Booking(ev.Id);
            bookingRepository.Add(booking);

            var service = new BookingBackgroundService(
                bookingRepository,
                eventRepository,
                NullLogger<BookingBackgroundService>.Instance);

            // Act
            await service.StartAsync(CancellationToken.None);

            await Task.Delay(TimeSpan.FromSeconds(3));

            await service.StopAsync(CancellationToken.None);

            var result = bookingRepository.GetById(booking.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Confirmed, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        [Fact]
        public async Task BackgroundService_EventDoesNotExist_RejectsBooking()
        {
            // Arrange
            var bookingRepository = new InMemoryBookingRepository();
            var eventRepository = new InMemoryEventRepository();

            var booking = new Booking(Guid.NewGuid());
            bookingRepository.Add(booking);

            var service = new BookingBackgroundService(
                bookingRepository,
                eventRepository,
                NullLogger<BookingBackgroundService>.Instance);

            // Act
            await service.StartAsync(CancellationToken.None);

            await Task.Delay(TimeSpan.FromSeconds(3));

            await service.StopAsync(CancellationToken.None);

            var result = bookingRepository.GetById(booking.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Rejected, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }
    }
}
