using WebApiProject.Entities;
using WebApiProject.Interfaces;

namespace WebApiProject.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingBackgroundService> _logger;

        private static readonly TimeSpan ProcessingDelay =
            TimeSpan.FromSeconds(2);

        private static readonly TimeSpan PollingInterval =
            TimeSpan.FromSeconds(1);

        public BookingBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    IReadOnlyList<Guid> pendingBookingIds;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingRepository = scope.ServiceProvider
                            .GetRequiredService<IBookingRepository>();

                        pendingBookingIds =
                            await bookingRepository.GetPendingIdsAsync(
                                stoppingToken);
                    }

                    var tasks = pendingBookingIds
                        .Select(id => ProcessBookingAsync(
                            id,
                            stoppingToken));

                    await Task.WhenAll(tasks);

                    await Task.Delay(
                        PollingInterval,
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Booking background service stopped.");
            }
        }

        private async Task ProcessBookingAsync(
            Guid bookingId,
            CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(
                    ProcessingDelay,
                    stoppingToken);

                using var scope = _scopeFactory.CreateScope();

                var bookingRepository = scope.ServiceProvider
                    .GetRequiredService<IBookingRepository>();

                var eventRepository = scope.ServiceProvider
                    .GetRequiredService<IEventRepository>();

                var booking = await bookingRepository.GetByIdAsync(
                    bookingId,
                    stoppingToken);

                if (booking == null)
                {
                    _logger.LogWarning(
                        "Booking {BookingId} no longer exists",
                        bookingId);

                    return;
                }

                var ev = await eventRepository.GetByIdAsync(
                    booking.EventId,
                    stoppingToken);

                if (ev == null)
                {
                    booking.Reject();

                    await bookingRepository.SaveChangesAsync(
                        stoppingToken);

                    _logger.LogWarning(
                        "Booking {BookingId} rejected because event {EventId} no longer exists",
                        booking.Id,
                        booking.EventId);

                    return;
                }

                booking.Confirm();

                await bookingRepository.SaveChangesAsync(
                    stoppingToken);

                _logger.LogInformation(
                    "Booking {BookingId} confirmed",
                    booking.Id);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Processing booking {BookingId} was cancelled",
                    bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing booking {BookingId}",
                    bookingId);

                await RejectBookingAsync(
                    bookingId,
                    stoppingToken);
            }
        }

        private async Task RejectBookingAsync(
            Guid bookingId,
            CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var bookingRepository = scope.ServiceProvider
                .GetRequiredService<IBookingRepository>();

            var eventRepository = scope.ServiceProvider
                .GetRequiredService<IEventRepository>();

            var booking = await bookingRepository.GetByIdAsync(
                bookingId,
                stoppingToken);

            if (booking == null)
            {
                return;
            }

            booking.Reject();

            var ev = await eventRepository.GetByIdAsync(
                booking.EventId,
                stoppingToken);

            if (ev != null)
            {
                ev.ReleaseSeats();
            }

            await bookingRepository.SaveChangesAsync(
                stoppingToken);
        }
    }
}