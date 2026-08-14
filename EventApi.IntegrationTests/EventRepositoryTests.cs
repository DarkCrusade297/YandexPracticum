using Infrastructure.DataAccess;
using FluentAssertions;
using Infrastructure.Repositories.Event;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace EventManagerSystem.Tests;

[Collection("Postgres collection")]
public sealed class EventRepositoryTests
{
    private readonly PostgresTestcontainerFixture _fixture;

    public EventRepositoryTests(PostgresTestcontainerFixture fixture)
    {
        _fixture = fixture;
    }

    private static EventRepository CreateRepository(AppDbContext db)
    {
        return new EventRepository(db);
    }

    private static EventModel CreateEventDto(
        string title = "Test event",
        string description = "Test description",
        int totalSeats = 100)
    {
        return new EventModel(title, description, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), totalSeats);
    }

    [Fact]
    public async Task CreateEventAsync_ShouldCreateEventInRealPostgres()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateRepository(db);

        var dto = CreateEventDto();

        var created = await repository.CreateEventAsync(dto);
        await repository.SaveChangesAsync();

        created.Id.Should().NotBeEmpty();
        created.Title.Should().Be(dto.Title);
        created.Description.Should().Be(dto.Description);
        created.StartAt.Should().Be(dto.StartAt);
        created.EndAt.Should().Be(dto.EndAt);
        created.TotalSeats.Should().Be(dto.TotalSeats);
        created.AvailableSeats.Should().Be(dto.TotalSeats);

        var fromDb = await db.Events.SingleOrDefaultAsync(e => e.Id == created.Id);

        fromDb.Should().NotBeNull();
        fromDb!.Title.Should().Be(dto.Title);
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnEvent_WhenExists()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateRepository(db);

        var created = await repository.CreateEventAsync(CreateEventDto(title: "Event by id"));

        await repository.SaveChangesAsync();

        var result = await repository.GetEventByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Title.Should().Be("Event by id");
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateRepository(db);

        var result = await repository.GetEventByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllEventsAsync_ShouldReturnAllEvents()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateRepository(db);

        await repository.CreateEventAsync(CreateEventDto(title: "Event 1"));
        await repository.CreateEventAsync(CreateEventDto(title: "Event 2"));
        await repository.CreateEventAsync(CreateEventDto(title: "Event 3"));

        await repository.SaveChangesAsync();

        var result = await repository.GetAllEventsAsync();

        result.Should().HaveCount(3);
        result.Select(e => e.Title).Should().Contain(["Event 1", "Event 2", "Event 3"]);
    }

    [Fact]
    public async Task UpdateEventAsync_ShouldUpdateEventInRealPostgres()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateRepository(db);

        var created = await repository.CreateEventAsync(CreateEventDto(title: "Old title"));

        await repository.SaveChangesAsync();

        var newStartAt = DateTime.UtcNow.AddDays(10);
        var newEndAt = newStartAt.AddHours(3);

        created.UpdateEvent("Updated title", "Updated description", newStartAt, newEndAt);

        repository.UpdateEvent(created);
        await repository.SaveChangesAsync();

        var fromDb = await db.Events.SingleAsync(e => e.Id == created.Id);

        fromDb.Title.Should().Be("Updated title");
        fromDb.Description.Should().Be("Updated description");
        fromDb.StartAt.Should().Be(newStartAt);
        fromDb.EndAt.Should().Be(newEndAt);
    }

    [Fact]
    public async Task DeleteEventAsync_ShouldDeleteEventFromRealPostgres()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateRepository(db);

        var created = await repository.CreateEventAsync(CreateEventDto(title: "To delete"));
        await repository.SaveChangesAsync();

        repository.DeleteEvent(created);
        await repository.SaveChangesAsync();

        var fromDb = await db.Events.SingleOrDefaultAsync(e => e.Id == created.Id);

        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistModifiedEvent()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateRepository(db);

        var created = await repository.CreateEventAsync(CreateEventDto(title: "Before save"));
        await repository.SaveChangesAsync();

        created.UpdateEvent(
            "After save",
            "Changed by SaveChangesAsync",
            created.StartAt,
            created.EndAt);

        repository.UpdateEvent(created);
        await repository.SaveChangesAsync();

        var fromDb = await db.Events.SingleAsync(e => e.Id == created.Id);

        fromDb.Title.Should().Be("After save");
        fromDb.Description.Should().Be("Changed by SaveChangesAsync");
    }
}
