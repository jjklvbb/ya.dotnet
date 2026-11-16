using EventApi.Application.Ports;
using EventApi.Application.Interfaces;

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
                await Task.Delay(ProcessingDelay, stoppingToken);

                using var scope = _scopeFactory.CreateScope();

                var processor = scope.ServiceProvider
                    .GetRequiredService<IBookingProcessingService>();

                await processor.ProcessAsync(bookingId, stoppingToken);
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

                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var processor = scope.ServiceProvider
                        .GetRequiredService<IBookingProcessingService>();

                    await processor.RejectAsync(bookingId, stoppingToken);
                }
                catch (Exception rejectionException)
                {
                    _logger.LogError(
                        rejectionException,
                        "Unable to reject booking {BookingId}",
                        bookingId);
                }
            }
        }
    }
}