using WebApiProject.DataAccess;
using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Services;

namespace WebApiProject.Test
{
    public class BookingServiceTest
    {
        private readonly InMemoryEventRepository _eventRepository;
        private readonly EventService _eventService;
        private readonly InMemoryBookingRepository _bookingRepository;
        private readonly BookingService _bookingService;
        private readonly Event _event;

        public BookingServiceTest()
        {
            _eventRepository = new InMemoryEventRepository();
            _eventService = new EventService(_eventRepository);

            _bookingRepository = new InMemoryBookingRepository();
            _bookingService = new BookingService(
                _bookingRepository,
                _eventService);

            _event = new Event(
                Guid.NewGuid(),
                "Test event",
                null,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(2),
                10);

            _eventService.CreateEvent(_event);
        }

        [Fact]
        public async Task CreateBooking_ExistingEvent_ReturnsPendingBooking()
        {
            // Arrange
            var eventId = _event.Id;

            // Act
            var result = await _bookingService.CreateBookingAsync(eventId);

            // Assert
            Assert.Equal(eventId, result.EventId);
            Assert.Equal(BookingStatus.Pending, result.Status);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Null(result.ProcessedAt);
        }

        [Fact]
        public async Task CreateBooking_SameEventTwice_ReturnsUniqueIds()
        {
            // Arrange
            var eventId = _event.Id;

            // Act
            var firstBooking = await _bookingService.CreateBookingAsync(eventId);
            var secondBooking = await _bookingService.CreateBookingAsync(eventId);

            // Assert
            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        }

        [Fact]
        public async Task GetBookingById_ExistingBooking_ReturnsBooking()
        {
            // Arrange
            var createdBooking = await _bookingService.CreateBookingAsync(_event.Id);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

            // Assert
            Assert.Equal(createdBooking.Id, result.Id);
            Assert.Equal(createdBooking.EventId, result.EventId);
            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task GetBookingById_AfterConfirm_ReturnsUpdatedStatus()
        {
            // Arrange
            var createdBooking = await _bookingService.CreateBookingAsync(_event.Id);
            var booking = _bookingRepository.GetById(createdBooking.Id)!;

            booking.Confirm();
            _bookingRepository.Update(booking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

            // Assert
            Assert.Equal(BookingStatus.Confirmed, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingById_AfterReject_ReturnsUpdatedStatus()
        {
            // Arrange
            var createdBooking = await _bookingService.CreateBookingAsync(_event.Id);
            var booking = _bookingRepository.GetById(createdBooking.Id)!;

            booking.Reject();
            _bookingRepository.Update(booking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

            // Assert
            Assert.Equal(BookingStatus.Rejected, result.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        [Fact]
        public async Task CreateBooking_NonExistingEvent_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistingEventId = Guid.NewGuid();

            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _bookingService.CreateBookingAsync(nonExistingEventId));

            // Assert
            Assert.Contains(nonExistingEventId.ToString(), exception.Message);
        }

        [Fact]
        public async Task CreateBooking_DeletedEvent_ThrowsNotFoundException()
        {
            // Arrange
            var eventId = _event.Id;
            _eventService.DeleteEvent(eventId);

            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _bookingService.CreateBookingAsync(eventId));

            // Assert
            Assert.Contains(eventId.ToString(), exception.Message);
        }

        [Fact]
        public async Task GetBookingById_NonExistingBooking_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistingBookingId = Guid.NewGuid();

            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _bookingService.GetBookingByIdAsync(nonExistingBookingId));

            // Assert
            Assert.Contains(nonExistingBookingId.ToString(), exception.Message);
        }
    }
}
