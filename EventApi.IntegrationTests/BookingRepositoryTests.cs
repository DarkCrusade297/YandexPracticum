using EventManagerSystem.DataAccess;
using EventManagerSystem.DTO.Events;
using EventManagerSystem.Enums;
using EventManagerSystem.Models;
using EventManagerSystem.Repositories.Booking;
using EventManagerSystem.Repositories.Event;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventManagerSystem.Tests;

[Collection("Postgres collection")]
public sealed class BookingRepositoryTests
{
    private readonly PostgresTestcontainerFixture _fixture;

    public BookingRepositoryTests(PostgresTestcontainerFixture fixture)
    {
        _fixture = fixture;
    }

    private static BookingRepository CreateBookingRepository(AppDbContext db)
    {
        return new BookingRepository(db);
    }

    private static EventRepository CreateEventRepository(AppDbContext db)
    {
        return new EventRepository(db);
    }

    private static async Task<EventModel> CreateEventAsync(AppDbContext db)
    {
        var eventRepository = CreateEventRepository(db);

        return await eventRepository.CreateEventAsync(new CreateEventDto
        {
            Title = "Booking test event",
            Description = "Event for booking tests",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            TotalSeats = 100
        });
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreateBookingInRealPostgres()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var eventModel = await CreateEventAsync(db);
        var repository = CreateBookingRepository(db);

        var booking = new BookingModel(eventModel.Id, processedAt: null);

        var created = await repository.CreateBookingAsync(booking);

        created.Id.Should().NotBeEmpty();
        created.EventId.Should().Be(eventModel.Id);
        created.Status.Should().Be(BookingStatus.Pending);
        created.CreatedAt.Should().NotBe(default);

        var fromDb = await db.Bookings.SingleOrDefaultAsync(b => b.Id == created.Id);

        fromDb.Should().NotBeNull();
        fromDb!.EventId.Should().Be(eventModel.Id);
        fromDb.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ShouldReturnBooking_WhenExists()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var eventModel = await CreateEventAsync(db);
        var repository = CreateBookingRepository(db);

        var booking = await repository.CreateBookingAsync(
            new BookingModel(eventModel.Id, processedAt: null));

        var result = await repository.GetBookingByIdAsync(booking.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(booking.Id);
        result.EventId.Should().Be(eventModel.Id);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateBookingRepository(db);

        var result = await repository.GetBookingByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPendingBookingsAsync_ShouldReturnOnlyPendingBookings()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var eventModel = await CreateEventAsync(db);
        var repository = CreateBookingRepository(db);

        var pending1 = await repository.CreateBookingAsync(
            new BookingModel(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, processedAt: null));

        var pending2 = await repository.CreateBookingAsync(
            new BookingModel(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, processedAt: null));

        var result = await repository.GetPendingBookingsAsync();

        result.Should().HaveCount(2);
        result.Select(b => b.Id).Should().Contain(pending1.Id);
        result.Select(b => b.Id).Should().Contain(pending2.Id);
        result.All(b => b.Status == BookingStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public async Task GetPendingBookingsIdsAsync_ShouldReturnOnlyPendingBookingIds()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var eventModel = await CreateEventAsync(db);
        var repository = CreateBookingRepository(db);

        var pending1 = await repository.CreateBookingAsync(
            new BookingModel(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, processedAt: null));

        var pending2 = await repository.CreateBookingAsync(
            new BookingModel(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, processedAt: null));

        var confirmed = await repository.CreateBookingAsync(
            new BookingModel(Guid.NewGuid(), eventModel.Id, BookingStatus.Confirmed, processedAt: DateTime.UtcNow));

        var ids = await repository.GetPendingBookingsIdsAsync();

        ids.Should().HaveCount(2);
        ids.Should().Contain(pending1.Id);
        ids.Should().Contain(pending2.Id);
        ids.Should().NotContain(confirmed.Id);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistModifiedBooking()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var eventModel = await CreateEventAsync(db);
        var repository = CreateBookingRepository(db);

        var booking = await repository.CreateBookingAsync(
            new BookingModel(eventModel.Id, processedAt: null));

        booking.Status = BookingStatus.Confirmed;
        booking.ProcessedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync();

        var fromDb = await db.Bookings.SingleAsync(b => b.Id == booking.Id);

        fromDb.Status.Should().Be(BookingStatus.Confirmed);
        fromDb.ProcessedAt.Should().NotBeNull();
    }
}
