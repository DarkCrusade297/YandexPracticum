using System.ComponentModel.DataAnnotations;
using Event.Application.Common.Caching;
using Event.Application.Common.Interfaces;
using Event.Domain.Exceptions;
using Event.Infrastructure.DataAccess;
using Event.Infrastructure.Entities;
using Messaging.Contracts.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Messaging;

public sealed class BookingConfirmedProcessor(
    EventDbContext db,
    IEventRepository eventRepository,
    ICacheService cacheService)
{
    public async Task<BookingConfirmedProcessingResult> ProcessAsync(
        BookingConfirmed message,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        if (await db.ProcessedBookings.AnyAsync(
                processed => processed.BookingId == message.BookingId,
                cancellationToken))
        {
            await transaction.CommitAsync(cancellationToken);
            return BookingConfirmedProcessingResult.Duplicate;
        }

        var eventModel = await eventRepository.GetEventByIdAsync(message.EventId);
        if (eventModel is null)
        {
            await MarkAsProcessedAsync(message, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return BookingConfirmedProcessingResult.EventNotFound;
        }

        BookingConfirmedProcessingResult result;
        try
        {
            eventModel.BookSeat(message.SeatCount);
            eventRepository.UpdateEvent(eventModel);
            result = BookingConfirmedProcessingResult.Processed;
        }
        catch (NoAvailableSeatsException)
        {
            result = BookingConfirmedProcessingResult.NoAvailableSeats;
        }
        catch (ValidationException)
        {
            result = BookingConfirmedProcessingResult.InvalidSeatCount;
        }

        db.ProcessedBookings.Add(CreateProcessedBooking(message));
        await eventRepository.SaveChangesAsync();
        await transaction.CommitAsync(cancellationToken);

        if (result == BookingConfirmedProcessingResult.Processed)
            await cacheService.RemoveAsync(EventCacheKeys.ById(message.EventId));

        return result;
    }

    private async Task MarkAsProcessedAsync(BookingConfirmed message, CancellationToken cancellationToken)
    {
        db.ProcessedBookings.Add(CreateProcessedBooking(message));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static ProcessedBookingEntity CreateProcessedBooking(BookingConfirmed message) => new()
    {
        BookingId = message.BookingId,
        EventId = message.EventId,
        ProcessedAt = DateTimeOffset.UtcNow
    };
}
