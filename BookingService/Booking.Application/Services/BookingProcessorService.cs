using Booking.Application.Common.Interfaces;
using Booking.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Booking.Application.Services;

public class BookingProcessorService : BackgroundService
{
    private readonly ILogger<BookingProcessorService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(40);

    public BookingProcessorService(ILogger<BookingProcessorService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessPendingBookingsAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Ошибка при обработке бронирований"); }
            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingBookingsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var ids = await repository.GetPendingBookingsIdsAsync();
        await Task.WhenAll(ids.Select(id => ProcessBookingAsync(id, cancellationToken)));
    }

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventGateway = scope.ServiceProvider.GetRequiredService<IEventGateway>();
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            var booking = await bookingService.GetBookingModelByIdAsync(bookingId);
            await eventGateway.GetEventAsync(booking.EventId, cancellationToken);
            await bookingService.UpdateBookingAsync(booking.Id);
        }
        catch (NotFoundException) { await bookingService.RejectBookingAsync(bookingId); }
        catch (Exception ex) { _logger.LogError(ex, "Ошибка при обработке бронирования {BookingId}", bookingId); }
    }
}
