using Booking.Domain.Enums;
using Booking.Domain.Models;
using Booking.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class BookingRepositoryTests(PostgresTestcontainerFixture fixture)
{
    [Fact]
    public async Task CreateBookingAsync_PersistsBooking()
    {
        await fixture.ResetBookingsAsync();
        await using var db = fixture.CreateBookingDbContext();
        var repository = new BookingRepository(db);
        var booking = CreateBooking();

        var created = await repository.CreateBookingAsync(booking);

        created.Id.Should().NotBeEmpty();
        var entity = await db.Bookings.SingleAsync(item => item.Id == booking.Id);
        entity.EventId.Should().Be(booking.EventId);
        entity.UserId.Should().Be(booking.UserId);
        entity.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ReturnsBookingWhenItExists()
    {
        await fixture.ResetBookingsAsync();
        await using var db = fixture.CreateBookingDbContext();
        var repository = new BookingRepository(db);
        var booking = await repository.CreateBookingAsync(CreateBooking());

        var result = await repository.GetBookingByIdAsync(booking.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(booking.Id);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ReturnsNullWhenItDoesNotExist()
    {
        await fixture.ResetBookingsAsync();
        await using var db = fixture.CreateBookingDbContext();
        var result = await new BookingRepository(db).GetBookingByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task PendingQueries_ReturnOnlyPendingBookings()
    {
        await fixture.ResetBookingsAsync();
        await using var db = fixture.CreateBookingDbContext();
        var repository = new BookingRepository(db);
        var pending1 = await repository.CreateBookingAsync(CreateBooking());
        var pending2 = await repository.CreateBookingAsync(CreateBooking());
        var confirmed = await repository.CreateBookingAsync(CreateBooking());
        confirmed.UpdateStatus(BookingStatus.Confirmed);
        repository.UpdateBooking(confirmed);
        await repository.SaveChangesAsync();

        var models = await repository.GetPendingBookingsAsync();
        var ids = await repository.GetPendingBookingsIdsAsync();

        models.Select(item => item.Id).Should().BeEquivalentTo([pending1.Id, pending2.Id]);
        ids.Should().BeEquivalentTo([pending1.Id, pending2.Id]);
    }

    [Fact]
    public async Task CountActiveBookingsByUserIdAsync_CountsPendingAndConfirmedOnly()
    {
        await fixture.ResetBookingsAsync();
        await using var db = fixture.CreateBookingDbContext();
        var repository = new BookingRepository(db);
        var userId = Guid.NewGuid();
        await repository.CreateBookingAsync(CreateBooking(userId));
        var confirmed = await repository.CreateBookingAsync(CreateBooking(userId));
        confirmed.UpdateStatus(BookingStatus.Confirmed);
        repository.UpdateBooking(confirmed);
        var cancelled = await repository.CreateBookingAsync(CreateBooking(userId));
        cancelled.UpdateStatus(BookingStatus.Cancelled);
        repository.UpdateBooking(cancelled);
        await repository.CreateBookingAsync(CreateBooking(Guid.NewGuid()));
        await repository.SaveChangesAsync();

        var count = await repository.CountActiveBookingsByUserIdAsync(userId);

        count.Should().Be(2);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsUpdatedStatus()
    {
        await fixture.ResetBookingsAsync();
        await using var db = fixture.CreateBookingDbContext();
        var repository = new BookingRepository(db);
        var booking = await repository.CreateBookingAsync(CreateBooking());
        booking.UpdateStatus(BookingStatus.Confirmed);
        repository.UpdateBooking(booking);

        await repository.SaveChangesAsync();

        var entity = await db.Bookings.SingleAsync(item => item.Id == booking.Id);
        entity.Status.Should().Be(BookingStatus.Confirmed);
        entity.ProcessedAt.Should().NotBeNull();
    }

    private static BookingModel CreateBooking(Guid? userId = null) => new(Guid.NewGuid(), userId ?? Guid.NewGuid());
}
