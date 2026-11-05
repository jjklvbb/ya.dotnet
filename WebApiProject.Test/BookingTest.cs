using EventApi.Domain.Entities;
using EventApi.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace WebApiProject.Test
{
    public class BookingTest
    {
        [Fact]
        public void Constructor_CreatesPendingBooking()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            // Act
            var userId = Guid.NewGuid();
            var booking = new Booking(eventId, userId);

            // Assert
            Assert.NotEqual(Guid.Empty, booking.Id);
            Assert.Equal(userId, booking.UserId);
            Assert.Equal(eventId, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Null(booking.ProcessedAt);
            Assert.True(booking.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenUserIdIsEmpty()
        {
            var eventId = Guid.NewGuid();

            Assert.Throws<ValidationException>(
                () => new Booking(eventId, Guid.Empty));
        }

        [Fact]
        public void Confirm_ChangesStatusAndSetsProcessedAt()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), userId);

            // Act
            booking.Confirm();

            // Assert
            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact]
        public void Reject_ChangesStatusAndSetsProcessedAt()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), userId);

            // Act
            booking.Reject();

            // Assert
            Assert.Equal(BookingStatus.Rejected, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact]
        public void Cancel_ShouldChangeStatusToCancelled()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), userId);

            // Act
            booking.Cancel();

            // Assert
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenAlreadyCancelled()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), userId);
            booking.Cancel();

            // Act & Assert
            Assert.Throws<ValidationException>(() => booking.Cancel());
        }

        [Fact]
        public void Confirm_ShouldThrow_WhenBookingCancelled()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), userId);
            booking.Cancel();

            // Act & Assert
            Assert.Throws<ValidationException>(() => booking.Confirm());
        }
    }
}
