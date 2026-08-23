using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
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
            var repository = new InMemoryBookingRepository();
            var booking = new Booking(Guid.NewGuid());

            repository.Add(booking);

            var service = new BookingBackgroundService(
                repository,
                NullLogger<BookingBackgroundService>.Instance);

            // Act
            await service.StartAsync(CancellationToken.None);

            await Task.Delay(TimeSpan.FromSeconds(3));

            await service.StopAsync(CancellationToken.None);

            var result = repository.GetById(booking.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Confirmed, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }
    }
}
