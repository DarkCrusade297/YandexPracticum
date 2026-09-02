using Booking.Domain.Enums;
using Booking.Domain.Exceptions;

namespace BookingServices.Tests;

public sealed class NegativeTests
{
    [Fact]
    public async Task CreateBookingAsync_UserAtLimit_ThrowsBookingLimitExceededException()
    {
        var userId = Guid.NewGuid();
        var existing = Enumerable.Range(0, BookingLimitExceededException.MaxActiveBookingsPerUser)
            .Select(_ => PositiveTests.CreateBooking(userId)).ToArray();
        var context = new BookingServiceTestContext(existing);

        await Assert.ThrowsAsync<BookingLimitExceededException>(() =>
            context.Service.CreateBookingAsync(Guid.NewGuid(), userId));
    }

    [Fact]
    public async Task GetBookingByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        var context = new BookingServiceTestContext();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            context.Service.GetBookingByIdAsync(Guid.NewGuid(), Guid.NewGuid(), UserRoles.User));
    }

    [Fact]
    public async Task GetBookingByIdAsync_OtherUser_ThrowsForbiddenOperationException()
    {
        var booking = PositiveTests.CreateBooking(Guid.NewGuid());
        var context = new BookingServiceTestContext(booking);
        await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
            context.Service.GetBookingByIdAsync(booking.Id, Guid.NewGuid(), UserRoles.User));
    }

    [Fact]
    public async Task CancelBookingAsync_OtherUser_ThrowsForbiddenOperationException()
    {
        var booking = PositiveTests.CreateBooking(Guid.NewGuid());
        var context = new BookingServiceTestContext(booking);
        await Assert.ThrowsAsync<ForbiddenOperationException>(() =>
            context.Service.CancelBookingAsync(booking.Id, Guid.NewGuid(), UserRoles.User));
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task CancelBookingAsync_AlreadyCancelled_ThrowsBookingCancelException()
    {
        var ownerId = Guid.NewGuid();
        var booking = PositiveTests.CreateBooking(ownerId);
        booking.UpdateStatus(BookingStatus.Cancelled);
        var context = new BookingServiceTestContext(booking);
        await Assert.ThrowsAsync<BookingCancelException>(() =>
            context.Service.CancelBookingAsync(booking.Id, ownerId, UserRoles.User));
    }

    [Fact]
    public async Task UpdateBookingAsync_NonExistingId_ThrowsNotFoundException()
    {
        var context = new BookingServiceTestContext();
        await Assert.ThrowsAsync<NotFoundException>(() => context.Service.UpdateBookingAsync(Guid.NewGuid()));
        Assert.Empty(context.Publisher.Messages);
    }
}
