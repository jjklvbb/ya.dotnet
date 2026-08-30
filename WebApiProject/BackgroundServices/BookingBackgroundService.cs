using WebApiProject.Interfaces;

namespace WebApiProject.BackgroundServices
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingBackgroundService> _logger;

        public BookingBackgroundService(
            IBookingRepository bookingRepository,
            ILogger<BookingBackgroundService> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var pendingBookings = _bookingRepository.GetPending();

                foreach (var booking in pendingBookings)
                {
                    try
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(2),
                            stoppingToken);

                        booking.Confirm();
                        _bookingRepository.Update(booking);

                        _logger.LogInformation(
                            "Booking {BookingId} confirmed",
                            booking.Id);
                    }
                    catch (OperationCanceledException)
                        when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error while processing booking {BookingId}",
                            booking.Id);
                    }
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(1),
                    stoppingToken);
            }
        }
    }
}
