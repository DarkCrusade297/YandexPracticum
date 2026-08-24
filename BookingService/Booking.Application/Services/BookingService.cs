using Booking.Application.Common.Interfaces;
using Booking.Application.DTO.Bookings;
using Booking.Domain.Enums;
using Booking.Domain.Exceptions;
using Booking.Domain.Models;

namespace Booking.Application.Services;

public class BookingService : IBookingService
{
    private readonly IEventGateway _eventGateway;
    private readonly IBookingRepository _bookingRepository;
    private static readonly SemaphoreSlim BookingLock = new(1, 1);

    public BookingService(IEventGateway eventGateway, IBookingRepository bookingRepository)
    {
        _eventGateway = eventGateway;
        _bookingRepository = bookingRepository;
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
            var ev = await _eventGateway.GetEventAsync(eventId);
            if (ev.StartAt < DateTime.UtcNow) throw new EventAlreadyPassedException(eventId);
            var activeCount = await _bookingRepository.CountActiveBookingsByUserIdAsync(userId);
            if (activeCount >= BookingLimitExceededException.MaxActiveBookingsPerUser)
                throw new BookingLimitExceededException(userId);
            await _eventGateway.ReserveSeatAsync(eventId);
            try
            {
                var created = await _bookingRepository.CreateBookingAsync(new BookingModel(eventId, userId));
                return new CreatedBookingDto
                {
                    Id = created.Id,
                    EventId = created.EventId,
                    UserId = created.UserId,
                    Status = created.Status
                };
            }
            catch
            {
                await _eventGateway.ReleaseSeatAsync(eventId);
                throw;
            }
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

    public async Task UpdateBookingAsync(Guid bookingId)
    {
        await BookingLock.WaitAsync();
        try
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId)
                ?? throw new NotFoundException($"Booking with id {bookingId} not found");
            booking.UpdateStatus(BookingStatus.Confirmed);
            _bookingRepository.UpdateBooking(booking);
            await _bookingRepository.SaveChangesAsync();
        }
        finally { BookingLock.Release(); }
    }

    public async Task RejectBookingAsync(Guid bookingId)
    {
        await BookingLock.WaitAsync();
        try
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId)
                ?? throw new NotFoundException($"Booking with id {bookingId} not found");
            await _eventGateway.ReleaseSeatAsync(booking.EventId);
            booking.UpdateStatus(BookingStatus.Rejected);
            _bookingRepository.UpdateBooking(booking);
            await _bookingRepository.SaveChangesAsync();
        }
        finally { BookingLock.Release(); }
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
            await _eventGateway.ReleaseSeatAsync(booking.EventId);
            booking.UpdateStatus(BookingStatus.Cancelled);
            _bookingRepository.UpdateBooking(booking);
            await _bookingRepository.SaveChangesAsync();
        }
        finally { BookingLock.Release(); }
    }
}
