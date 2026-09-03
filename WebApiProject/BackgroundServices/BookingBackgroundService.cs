using WebApiProject.Entities;
using WebApiProject.Interfaces;

namespace WebApiProject.BackgroundServices
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<BookingBackgroundService> _logger;

        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

        private static readonly TimeSpan ProcessingDelay =
            TimeSpan.FromSeconds(2);

        private static readonly TimeSpan PollingInterval =
            TimeSpan.FromSeconds(1);

        public BookingBackgroundService(IBookingRepository bookingRepository, IEventRepository eventRepository,
            ILogger<BookingBackgroundService> logger)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var pendingBookings = _bookingRepository
                        .GetPending()
                        .ToList();

                    var tasks = pendingBookings
                        .Select(booking => ProcessBookingAsync(booking, stoppingToken));

                    await Task.WhenAll(tasks);

                    await Task.Delay(PollingInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Booking background service stopped.");
            }
        }

        private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {
            Event? ev = null;

            try
            {
                await Task.Delay(ProcessingDelay, stoppingToken);

                await _processingSemaphore.WaitAsync(stoppingToken);

                try
                {
                    ev = _eventRepository.GetById(booking.EventId);

                    if (ev == null)
                    {
                        booking.Reject();
                        _bookingRepository.Update(booking);

                        _logger.LogWarning(
                            "Booking {BookingId} rejected because event {EventId} no longer exists",
                            booking.Id,
                            booking.EventId);

                        return;
                    }

                    booking.Confirm();
                    _bookingRepository.Update(booking);

                    _logger.LogInformation(
                        "Booking {BookingId} confirmed",
                        booking.Id);
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Processing booking {BookingId} was cancelled",
                    booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing booking {BookingId}",
                    booking.Id);

                await RejectBookingAsync(
                    booking,
                    ev,
                    stoppingToken);
            }
        }

        private async Task RejectBookingAsync(Booking booking, Event? ev, CancellationToken stoppingToken)
        {
            await _processingSemaphore.WaitAsync(stoppingToken);

            try
            {
                booking.Reject();
                _bookingRepository.Update(booking);

                if (ev != null)
                {
                    ev.ReleaseSeats();
                    _eventRepository.Update(ev);
                }
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }
    }
}
