using Booking.Application.Common.Interfaces;
using Booking.Application.Services;
using Booking.Domain.Enums;
using Booking.Domain.Exceptions;
using Booking.Domain.Models;
using Messaging.Contracts.Bookings;

namespace BookingServices.Tests;

public sealed class PositiveTests
{
    [Fact]
    public async Task CreateBookingAsync_ValidRequest_ReturnsPendingBooking()
    {
        var context = new BookingServiceTestContext();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = await context.Service.CreateBookingAsync(eventId, userId);

        Assert.NotNull(result);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Contains(context.Repository.Bookings, item => item.Id == result.Id);
    }

    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_ReturnUniqueBookingsUpToUserLimit()
    {
        var context = new BookingServiceTestContext();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var results = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => context.Service.CreateBookingAsync(eventId, userId)));

        Assert.Equal(10, results.Length);
        Assert.Equal(10, results.Select(item => item!.Id).Distinct().Count());
        Assert.All(results, item => Assert.Equal(BookingStatus.Pending, item!.Status));
    }

    [Fact]
    public async Task CreateBookingAsync_TwoUsersHaveIndependentLimits()
    {
        var context = new BookingServiceTestContext();
        var firstUser = Guid.NewGuid();
        var secondUser = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        for (var index = 0; index < BookingLimitExceededException.MaxActiveBookingsPerUser; index++)
            await context.Service.CreateBookingAsync(eventId, firstUser);

        var secondUserBooking = await context.Service.CreateBookingAsync(eventId, secondUser);

        Assert.NotNull(secondUserBooking);
        Assert.Equal(secondUser, secondUserBooking.UserId);
    }

    [Fact]
    public async Task GetBookingByIdAsync_OwnerCanReadBooking()
    {
        var ownerId = Guid.NewGuid();
        var booking = CreateBooking(ownerId);
        var context = new BookingServiceTestContext(booking);

        var result = await context.Service.GetBookingByIdAsync(booking.Id, ownerId, UserRoles.User);

        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetBookingByIdAsync_AdminCanReadAnotherUsersBooking()
    {
        var booking = CreateBooking(Guid.NewGuid());
        var context = new BookingServiceTestContext(booking);

        var result = await context.Service.GetBookingByIdAsync(booking.Id, Guid.NewGuid(), UserRoles.Admin);

        Assert.Equal(booking.Id, result!.Id);
    }

    [Fact]
    public async Task UpdateBookingAsync_ConfirmsPersistsThenPublishesMessage()
    {
        var booking = CreateBooking(Guid.NewGuid());
        var context = new BookingServiceTestContext(booking);

        await context.Service.UpdateBookingAsync(booking.Id);

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
        var message = Assert.Single(context.Publisher.Messages);
        Assert.Equal(booking.Id, message.BookingId);
        Assert.Equal(booking.EventId, message.EventId);
        Assert.True(context.Operations.IndexOf("repository.save") < context.Operations.IndexOf("publisher.publish"));
    }

    [Fact]
    public async Task GetPendingBookingsAsync_ReturnsOnlyPendingBookings()
    {
        var pending = CreateBooking(Guid.NewGuid());
        var confirmed = CreateBooking(Guid.NewGuid());
        confirmed.UpdateStatus(BookingStatus.Confirmed);
        var context = new BookingServiceTestContext(pending, confirmed);

        var result = (await context.Service.GetPendingBookingsAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(pending.Id, result[0].Id);
    }

    [Fact]
    public async Task CancelBookingAsync_OwnerCancelsBooking()
    {
        var ownerId = Guid.NewGuid();
        var booking = CreateBooking(ownerId);
        var context = new BookingServiceTestContext(booking);

        await context.Service.CancelBookingAsync(booking.Id, ownerId, UserRoles.User);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
        Assert.Contains("repository.save", context.Operations);
    }

    [Fact]
    public async Task CancelBookingAsync_AdminCancelsAnotherUsersBooking()
    {
        var booking = CreateBooking(Guid.NewGuid());
        var context = new BookingServiceTestContext(booking);

        await context.Service.CancelBookingAsync(booking.Id, Guid.NewGuid(), UserRoles.Admin);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    internal static BookingModel CreateBooking(Guid userId) => new(Guid.NewGuid(), userId);
}

internal sealed class BookingServiceTestContext
{
    public List<string> Operations { get; } = [];
    public StubBookingRepository Repository { get; }
    public StubBookingConfirmedPublisher Publisher { get; }
    public BookingService Service { get; }

    public BookingServiceTestContext(params BookingModel[] bookings)
    {
        Repository = new StubBookingRepository(Operations, bookings);
        Publisher = new StubBookingConfirmedPublisher(Operations);
        Service = new BookingService(Repository, Publisher);
    }
}

internal sealed class StubBookingRepository(List<string> operations, IEnumerable<BookingModel> bookings) : IBookingRepository
{
    private readonly object _sync = new();
    public List<BookingModel> Bookings { get; } = [.. bookings];

    public Task<BookingModel?> GetBookingByIdAsync(Guid bookingId)
    {
        lock (_sync) return Task.FromResult(Bookings.FirstOrDefault(item => item.Id == bookingId));
    }

    public Task<BookingModel> CreateBookingAsync(BookingModel booking)
    {
        lock (_sync) Bookings.Add(booking);
        operations.Add("repository.create");
        return Task.FromResult(booking);
    }

    public Task<int> CountActiveBookingsByUserIdAsync(Guid userId)
    {
        lock (_sync)
            return Task.FromResult(Bookings.Count(item => item.UserId == userId &&
                item.Status is BookingStatus.Pending or BookingStatus.Confirmed));
    }

    public Task<List<BookingModel>> GetPendingBookingsAsync()
    {
        lock (_sync) return Task.FromResult(Bookings.Where(item => item.Status == BookingStatus.Pending).ToList());
    }

    public Task<List<Guid>> GetPendingBookingsIdsAsync()
    {
        lock (_sync) return Task.FromResult(Bookings.Where(item => item.Status == BookingStatus.Pending).Select(item => item.Id).ToList());
    }

    public void UpdateBooking(BookingModel model) => operations.Add("repository.update");
    public Task SaveChangesAsync() { operations.Add("repository.save"); return Task.CompletedTask; }
}

internal sealed class StubBookingConfirmedPublisher(List<string> operations) : IBookingConfirmedPublisher
{
    public List<BookingConfirmed> Messages { get; } = [];
    public Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken = default)
    {
        operations.Add("publisher.publish");
        Messages.Add(message);
        return Task.CompletedTask;
    }
}
