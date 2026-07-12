using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using EventManagerSystem.Services.EventService;

namespace EventManagerSystem.Services.BookingService
{
    public class BookingProcessorService : BackgroundService
    {
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        private readonly ILogger<BookingProcessorService> _logger;
        private readonly IBookingService _bookingService;
        private readonly IEventService _eventService;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(40);
        public BookingProcessorService(
            ILogger<BookingProcessorService> logger,
            IBookingService bookingService,
            IEventService eventService)
        {
            _logger = logger;
            _bookingService = bookingService;
            _eventService = eventService;
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

        private async Task ProcessBookingAsync(BookingModel booking, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation($"Обработка бронирования {booking.Id}");

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                await _processingSemaphore.WaitAsync(new CancellationToken());
                try
                {
                    var _event = _eventService.GetEventAsync(booking.EventId);

                    await _bookingService.UpdateBookingAsync(booking);

                    _logger.LogWarning($"Бронирование {booking.Id} подтверждено");
                }
                catch (NotFoundException ex)
                {
                    await _bookingService.RejectBookingAsync(booking);
                    _logger.LogWarning($"Бронирование {booking.Id} отклонено");
                }
                finally
                { 
                    _processingSemaphore.Release(); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обработке бронирования {booking.Id}");
            }
        }

        private async Task ProcessPendingBookingsAsync(CancellationToken cancellationToken)
        {
            var pendingBookings = (await _bookingService.GetPendingBookingsAsync()).ToList();

            if (pendingBookings == null || !pendingBookings.Any())
            {
                _logger.LogInformation("Нет бронирований для обработки");
                return;
            }

            _logger.LogInformation($"Найдено {pendingBookings.Count()} бронирований для обработки");

            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }
}
