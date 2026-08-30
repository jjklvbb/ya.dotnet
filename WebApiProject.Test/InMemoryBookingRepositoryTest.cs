using WebApiProject.DataAccess;
using WebApiProject.Entities;

namespace WebApiProject.Test
{
    public class InMemoryBookingRepositoryTest
    {
        [Fact]
        public void Add_And_GetById_ReturnsBooking()
        {
            // Arrange
            var repository = new InMemoryBookingRepository();
            var booking = new Booking(Guid.NewGuid());

            // Act
            repository.Add(booking);
            var result = repository.GetById(booking.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(booking.Id, result.Id);
        }

        [Fact]
        public void GetById_NonExistingBooking_ReturnsNull()
        {
            // Arrange
            var repository = new InMemoryBookingRepository();

            // Act
            var result = repository.GetById(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetPending_ReturnsOnlyPendingBookings()
        {
            // Arrange
            var repository = new InMemoryBookingRepository();

            var pendingBooking = new Booking(Guid.NewGuid());

            var confirmedBooking = new Booking(Guid.NewGuid());
            confirmedBooking.Confirm();

            var rejectedBooking = new Booking(Guid.NewGuid());
            rejectedBooking.Reject();

            repository.Add(pendingBooking);
            repository.Add(confirmedBooking);
            repository.Add(rejectedBooking);

            // Act
            var result = repository.GetPending().ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(pendingBooking.Id, result[0].Id);
        }

        [Fact]
        public void Update_UpdatesExistingBooking()
        {
            // Arrange
            var repository = new InMemoryBookingRepository();
            var booking = new Booking(Guid.NewGuid());

            repository.Add(booking);
            booking.Confirm();

            // Act
            repository.Update(booking);
            var result = repository.GetById(booking.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Confirmed, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }
    }
}
