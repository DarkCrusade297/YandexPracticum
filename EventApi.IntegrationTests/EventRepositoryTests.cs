using EventManagerSystem.DataAccess;
using EventManagerSystem.DTO.Events;
using FluentAssertions;
using EventManagerSystem.Repositories.Event;
using Microsoft.EntityFrameworkCore;

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

    private static CreateEventDto CreateEventDto(
        string title = "Test event",
        string description = "Test description",
        int totalSeats = 100)
    {
        return new CreateEventDto
        {
            Title = title,
            Description = description,
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            TotalSeats = totalSeats
        };
    }

    [Fact]
    public async Task CreateEventAsync_ShouldCreateEventInRealPostgres()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateRepository(db);

        var dto = CreateEventDto();

        var created = await repository.CreateEventAsync(dto);

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

        var result = await repository.GetAllEventsAsync().ToListAsync();

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

        var updateDto = new UpdateEventDto
        {
            Title = "Updated title",
            Description = "Updated description",
            StartAt = DateTime.UtcNow.AddDays(10),
            EndAt = DateTime.UtcNow.AddDays(10).AddHours(3)
        };

        var updated = await repository.UpdateEventAsync(created.Id, updateDto);

        updated.Id.Should().Be(created.Id);
        updated.Title.Should().Be(updateDto.Title);
        updated.Description.Should().Be(updateDto.Description);
        updated.StartAt.Should().Be(updateDto.StartAt);
        updated.EndAt.Should().Be(updateDto.EndAt);

        var fromDb = await db.Events.SingleAsync(e => e.Id == created.Id);

        fromDb.Title.Should().Be(updateDto.Title);
        fromDb.Description.Should().Be(updateDto.Description);
    }

    [Fact]
    public async Task DeleteEventAsync_ShouldDeleteEventFromRealPostgres()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = CreateRepository(db);

        var created = await repository.CreateEventAsync(CreateEventDto(title: "To delete"));

        await repository.DeleteEventAsync(created);

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

        created.Title = "After save";
        created.Description = "Changed by SaveChangesAsync";

        await repository.SaveChangesAsync();

        var fromDb = await db.Events.SingleAsync(e => e.Id == created.Id);

        fromDb.Title.Should().Be("After save");
        fromDb.Description.Should().Be("Changed by SaveChangesAsync");
    }
}
