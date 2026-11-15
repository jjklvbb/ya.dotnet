using Bookings.Application.Interfaces;
using Bookings.Application.Ports;

namespace Bookings.Api.Services;

public sealed class BookingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingBackgroundService> _logger;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var repository = scope.ServiceProvider
                    .GetRequiredService<IBookingRepository>();

                var processor = scope.ServiceProvider
                    .GetRequiredService<IBookingProcessingService>();

                var pendingIds = await repository.GetPendingIdsAsync(
                    stoppingToken);

                foreach (var bookingId in pendingIds)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await processor.ProcessAsync(
                            bookingId, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Ошибка обработки брони {BookingId}",
                            bookingId);
                    }
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка фоновой обработки бронирований");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }
    }
}