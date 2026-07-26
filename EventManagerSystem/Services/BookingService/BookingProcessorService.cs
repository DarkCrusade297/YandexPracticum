using EventManagerSystem.DataAccess;
using EventManagerSystem.Enums;
using EventManagerSystem.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EventManagerSystem.Services.BookingService
{
    public class BookingProcessorService : BackgroundService
    {
        private readonly ILogger<BookingProcessorService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(40);

        public BookingProcessorService(
            ILogger<BookingProcessorService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
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

        private async Task<List<Guid>> GetPendingBookingIdsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await dbContext.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);
        }

        private async Task ProcessPendingBookingsAsync(CancellationToken cancellationToken)
        {
            var pendingBookingIds = await GetPendingBookingIdsAsync(cancellationToken);

            if (pendingBookingIds.Count == 0)
            {
                _logger.LogInformation("Нет бронирований для обработки");
                return;
            }

            _logger.LogInformation("Найдено {Count} бронирований для обработки", pendingBookingIds.Count);

            var tasks = pendingBookingIds.Select(id => ProcessBookingAsync(id, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

            try
            {
                _logger.LogInformation("Обработка бронирования {BookingId}", bookingId);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                var booking = await dbContext.Bookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId, stoppingToken);

                if (booking is null)
                {
                    _logger.LogWarning("Бронирование {BookingId} больше не существует, пропускаем", bookingId);
                    return;
                }

                try
                {
                    await eventService.GetEventAsync(booking.EventId);
                    await bookingService.UpdateBookingAsync(booking.Id);

                    _logger.LogInformation("Бронирование {BookingId} подтверждено", booking.Id);
                }
                catch (NotFoundException)
                {
                    await bookingService.RejectBookingAsync(booking.Id);
                    _logger.LogWarning("Бронирование {BookingId} отклонено", booking.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке бронирования {BookingId}", bookingId);
            }
        }
    }
}