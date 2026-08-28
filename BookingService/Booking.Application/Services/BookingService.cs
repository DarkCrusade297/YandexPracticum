using Booking.Application.Common.Interfaces;
using Booking.Application.DTO.Bookings;
using Booking.Domain.Enums;
using Booking.Domain.Exceptions;
using Booking.Domain.Models;
using Messaging.Contracts.Bookings;

namespace Booking.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingConfirmedPublisher _bookingConfirmedPublisher;
    private static readonly SemaphoreSlim BookingLock = new(1, 1);

    public BookingService(IBookingRepository bookingRepository, IBookingConfirmedPublisher bookingConfirmedPublisher)
    {
        _bookingRepository = bookingRepository;
        _bookingConfirmedPublisher = bookingConfirmedPublisher;
    }

    public async Task<BookingModel> GetBookingModelByIdAsync(Guid id)
    {
        await BookingLock.WaitAsync();
        try
        {
            return await _bookingRepository.GetBookingByIdAsync(id)
                ?? throw new NotFoundException($"Booking with id {id} not found");
        }
        finally { BookingLock.Release(); }
    }

    public async Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId, Guid userId)
    {
        await BookingLock.WaitAsync();
        try
        {
            var activeCount = await _bookingRepository.CountActiveBookingsByUserIdAsync(userId);
            if (activeCount >= BookingLimitExceededException.MaxActiveBookingsPerUser)
                throw new BookingLimitExceededException(userId);
            var created = await _bookingRepository.CreateBookingAsync(new BookingModel(eventId, userId));
            return new CreatedBookingDto
            {
                Id = created.Id,
                EventId = created.EventId,
                UserId = created.UserId,
                Status = created.Status
            };
        }
        finally { BookingLock.Release(); }
    }

    public async Task<GetBookingDto?> GetBookingByIdAsync(Guid bookingId, Guid currentUserId, UserRoles currentUserRole)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId)
            ?? throw new NotFoundException($"Booking with id {bookingId} not found");
        if (booking.UserId != currentUserId && currentUserRole != UserRoles.Admin)
            throw new ForbiddenOperationException($"User '{currentUserId}' has no permission to view booking '{bookingId}'");
        return GetBookingDto.FromDomain(booking);
    }

    public async Task<IEnumerable<BookingModel>> GetPendingBookingsAsync()
    {
        await BookingLock.WaitAsync();
        try { return await _bookingRepository.GetPendingBookingsAsync(); }
        finally { BookingLock.Release(); }
    }

    public async Task UpdateBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        BookingConfirmed message;
        await BookingLock.WaitAsync(cancellationToken);
        try
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId)
                ?? throw new NotFoundException($"Booking with id {bookingId} not found");
            booking.UpdateStatus(BookingStatus.Confirmed);
            _bookingRepository.UpdateBooking(booking);
            await _bookingRepository.SaveChangesAsync();
            var confirmedAt = booking.ProcessedAt
                ?? throw new InvalidOperationException($"Booking {booking.Id} has no processing timestamp.");
            message = new BookingConfirmed(
                booking.Id,
                booking.EventId,
                booking.UserId,
                SeatCount: 1,
                new DateTimeOffset(confirmedAt, TimeSpan.Zero));
        }
        finally { BookingLock.Release(); }

        await _bookingConfirmedPublisher.PublishAsync(message, cancellationToken);
    }

    public async Task CancelBookingAsync(Guid bookingId, Guid currentUserId, UserRoles currentUserRole)
    {
        await BookingLock.WaitAsync();
        try
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId)
                ?? throw new NotFoundException($"Booking with id {bookingId} not found");
            if (booking.UserId != currentUserId && currentUserRole != UserRoles.Admin)
                throw new ForbiddenOperationException($"User '{currentUserId}' has no permission to cancel booking '{bookingId}'");
            if (booking.Status == BookingStatus.Cancelled)
                throw new BookingCancelException($"Booking with id {bookingId} already was cancelled");
            booking.UpdateStatus(BookingStatus.Cancelled);
            _bookingRepository.UpdateBooking(booking);
            await _bookingRepository.SaveChangesAsync();
        }
        finally { BookingLock.Release(); }
    }
}
