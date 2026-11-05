using EventApi.Domain.Entities;
using EventApi.Domain.Exceptions;

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
            var booking = new Booking(eventId);

            // Assert
            Assert.NotEqual(Guid.Empty, booking.Id);
            Assert.Equal(eventId, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Null(booking.ProcessedAt);
            Assert.True(booking.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Confirm_ChangesStatusAndSetsProcessedAt()
        {
            // Arrange
            var booking = new Booking(Guid.NewGuid());

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
            var booking = new Booking(Guid.NewGuid());

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
            var booking = new Booking(Guid.NewGuid());

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
            var booking = new Booking(Guid.NewGuid());
            booking.Cancel();

            // Act & Assert
            Assert.Throws<ValidationException>(() => booking.Cancel());
        }

        [Fact]
        public void Confirm_ShouldThrow_WhenBookingCancelled()
        {
            // Arrange
            var booking = new Booking(Guid.NewGuid());
            booking.Cancel();

            // Act & Assert
            Assert.Throws<ValidationException>(() => booking.Confirm());
        }
    }
}
