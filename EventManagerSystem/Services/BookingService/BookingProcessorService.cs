namespace EventManagerSystem.Services.BookingService
{
    public class BookingProcessorService : BackgroundService
    {
        private readonly ILogger<BookingProcessorService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(2);
        public BookingProcessorService(
            ILogger<BookingProcessorService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingBookingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке бронирований");
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }
        }

        private async Task ProcessPendingBookingsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            var pendingBookings = await bookingService.GetPendingBookingsAsync();

            if (pendingBookings == null || !pendingBookings.Any())
            {
                _logger.LogInformation("Нет бронирований для обработки");
                return;
            }

            _logger.LogInformation($"Найдено {pendingBookings.Count()} бронирований для обработки");

            foreach (var booking in pendingBookings)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation($"Обработка бронирования {booking.Id}");

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                    await bookingService.ConfirmBookingAsync(booking.Id);

                    _logger.LogInformation($"Бронирование {booking.Id} подтверждено");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка при обработке бронирования {booking.Id}");
                }
            }
        }
    }
}
