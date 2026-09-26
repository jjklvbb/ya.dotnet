using Microsoft.EntityFrameworkCore;
using WebApiProject.DataAccess;
using WebApiProject.Entities;

namespace WebApiProject.BackgroundServices
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
                    List<Guid> pendingBookingIds;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider
                            .GetRequiredService<AppDbContext>();

                        pendingBookingIds = await context.Bookings
                            .AsNoTracking()
                            .Where(b => b.Status == BookingStatus.Pending)
                            .Select(b => b.Id)
                            .ToListAsync(stoppingToken);
                    }

                    var tasks = pendingBookingIds
                        .Select(id => ProcessBookingAsync(id, stoppingToken));

                    await Task.WhenAll(tasks);

                    await Task.Delay(PollingInterval, stoppingToken);
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

                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var booking = await context.Bookings
                    .FirstOrDefaultAsync(
                        b => b.Id == bookingId,
                        stoppingToken);

                if (booking == null)
                {
                    _logger.LogWarning(
                        "Booking {BookingId} no longer exists",
                        bookingId);

                    return;
                }

                var ev = await context.Events
                    .FirstOrDefaultAsync(
                        e => e.Id == booking.EventId,
                        stoppingToken);

                if (ev == null)
                {
                    booking.Reject();

                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogWarning(
                        "Booking {BookingId} rejected because event {EventId} no longer exists",
                        booking.Id,
                        booking.EventId);

                    return;
                }

                booking.Confirm();

                await context.SaveChangesAsync(stoppingToken);

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

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var booking = await context.Bookings
                .FirstOrDefaultAsync(
                    b => b.Id == bookingId,
                    stoppingToken);

            if (booking == null)
            {
                return;
            }

            booking.Reject();

            var ev = await context.Events
                .FirstOrDefaultAsync(
                    e => e.Id == booking.EventId,
                    stoppingToken);

            if (ev != null)
            {
                ev.ReleaseSeats();
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}