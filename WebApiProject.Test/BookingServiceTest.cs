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
            _bookingService = new BookingService(_bookingRepository, _eventRepository);

            _event = new Event(
                Guid.NewGuid(),
                "Test event",
                null,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(2),
                10);

            _eventService.CreateEvent(_event);
        }

        private Event CreateTestEvent(int totalSeats = 10)
        {
            var ev = new Event(
                Guid.NewGuid(),
                "Test event",
                null,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(2),
                totalSeats);

            _eventRepository.Add(ev);

            return ev;
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

        [Fact]
        public async Task CreateBooking_ExistingEvent_DecreasesAvailableSeats()
        {
            // Arrange
            var ev = CreateTestEvent(3);

            // Act
            await _bookingService.CreateBookingAsync(ev.Id);

            // Assert
            var updatedEvent = _eventRepository.GetById(ev.Id);

            Assert.NotNull(updatedEvent);
            Assert.Equal(2, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task CreateBooking_NoAvailableSeats_ThrowsNoAvailableSeatsException()
        {
            // Arrange
            var ev = CreateTestEvent(1);

            await _bookingService.CreateBookingAsync(ev.Id);

            // Act
            var act = () => _bookingService.CreateBookingAsync(ev.Id);

            // Assert
            await Assert.ThrowsAsync<NoAvailableSeatsException>(act);

            Assert.Equal(0, ev.AvailableSeats);
        }

        [Fact]
        public async Task CreateBooking_ConcurrentRequests_PreventsOverbooking()
        {
            // Arrange
            var ev = CreateTestEvent(5);

            var tasks = Enumerable.Range(0, 20)
                .Select(_ => Task.Run(async () =>
                {
                    try
                    {
                        await _bookingService.CreateBookingAsync(ev.Id);
                        return true;
                    }
                    catch (NoAvailableSeatsException)
                    {
                        return false;
                    }
                }))
                .ToArray();

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(5, results.Count(result => result));
            Assert.Equal(15, results.Count(result => !result));
            Assert.Equal(0, ev.AvailableSeats);
        }

        [Fact]
        public async Task CreateBooking_ConcurrentRequests_CreatesUniqueIds()
        {
            // Arrange
            var ev = CreateTestEvent(10);

            var tasks = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(
                    () => _bookingService.CreateBookingAsync(ev.Id)))
                .ToArray();

            // Act
            var bookings = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, bookings.Length);
            Assert.Equal(
                10,
                bookings.Select(booking => booking.Id).Distinct().Count());

            Assert.Equal(0, ev.AvailableSeats);
        }

        [Fact]
        public async Task RejectBooking_ReleaseSeats_RestoresAvailableSeats()
        {
            // Arrange
            var ev = CreateTestEvent(1);

            var bookingInfo = await _bookingService.CreateBookingAsync(ev.Id);

            Assert.Equal(0, ev.AvailableSeats);

            var booking = _bookingRepository.GetById(bookingInfo.Id);

            Assert.NotNull(booking);

            // Act
            booking.Reject();
            _bookingRepository.Update(booking);

            ev.ReleaseSeats();
            _eventRepository.Update(ev);

            // Assert
            Assert.Equal(BookingStatus.Rejected, booking.Status);
            Assert.Equal(1, ev.AvailableSeats);
        }

        [Fact]
        public async Task RejectBooking_ReleaseSeats_AllowsNewBooking()
        {
            // Arrange
            var ev = CreateTestEvent(1);

            var firstBookingInfo =
                await _bookingService.CreateBookingAsync(ev.Id);

            var firstBooking =
                _bookingRepository.GetById(firstBookingInfo.Id);

            Assert.NotNull(firstBooking);

            firstBooking.Reject();
            _bookingRepository.Update(firstBooking);

            ev.ReleaseSeats();
            _eventRepository.Update(ev);

            // Act
            var secondBooking =
                await _bookingService.CreateBookingAsync(ev.Id);

            // Assert
            Assert.NotNull(secondBooking);
            Assert.Equal(BookingStatus.Pending, secondBooking.Status);
            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
            Assert.Equal(0, ev.AvailableSeats);
        }
    }
}
