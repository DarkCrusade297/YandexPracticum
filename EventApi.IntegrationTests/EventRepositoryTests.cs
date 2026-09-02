using Event.Domain.Models;
using Event.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class EventRepositoryTests(PostgresTestcontainerFixture fixture)
{
    [Fact]
    public async Task CreateEventAsync_PersistsEvent()
    {
        await fixture.ResetEventsAsync();
        await using var db = fixture.CreateEventDbContext();
        var repository = new EventRepository(db);
        var model = CreateEvent("Created event");

        await repository.CreateEventAsync(model);
        await repository.SaveChangesAsync();

        var entity = await db.Events.SingleAsync(item => item.Id == model.Id);
        entity.Title.Should().Be(model.Title);
        entity.AvailableSeats.Should().Be(model.TotalSeats);
    }

    [Fact]
    public async Task GetEventByIdAsync_ReturnsEventWhenItExists()
    {
        await fixture.ResetEventsAsync();
        await using var db = fixture.CreateEventDbContext();
        var repository = new EventRepository(db);
        var model = CreateEvent("Event by id");
        await repository.CreateEventAsync(model);
        await repository.SaveChangesAsync();

        var result = await repository.GetEventByIdAsync(model.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Event by id");
    }

    [Fact]
    public async Task GetEventByIdAsync_ReturnsNullWhenItDoesNotExist()
    {
        await fixture.ResetEventsAsync();
        await using var db = fixture.CreateEventDbContext();
        var result = await new EventRepository(db).GetEventByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsAllEvents()
    {
        await fixture.ResetEventsAsync();
        await using var db = fixture.CreateEventDbContext();
        var repository = new EventRepository(db);
        await repository.CreateEventAsync(CreateEvent("Event 1"));
        await repository.CreateEventAsync(CreateEvent("Event 2"));
        await repository.CreateEventAsync(CreateEvent("Event 3"));
        await repository.SaveChangesAsync();

        var result = await repository.GetAllEventsAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetTopEventsAsync_OrdersBySoldPercentageAndLimitsResult()
    {
        await fixture.ResetEventsAsync();
        await using var db = fixture.CreateEventDbContext();
        var repository = new EventRepository(db);
        var events = Enumerable.Range(0, 12)
            .Select(index => CreateEvent($"Event {index}", totalSeats: 100, availableSeats: 100 - index * 5))
            .ToList();
        foreach (var model in events) await repository.CreateEventAsync(model);
        await repository.SaveChangesAsync();

        var result = await repository.GetTopEventsAsync(10);

        result.Should().HaveCount(10);
        result[0].AvailableSeats.Should().Be(45);
        result.Select(item => (double)(item.TotalSeats - item.AvailableSeats) / item.TotalSeats)
            .Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task UpdateEvent_PersistsChanges()
    {
        await fixture.ResetEventsAsync();
        await using var db = fixture.CreateEventDbContext();
        var repository = new EventRepository(db);
        var model = CreateEvent("Before update");
        await repository.CreateEventAsync(model);
        await repository.SaveChangesAsync();
        var start = DateTime.UtcNow.AddDays(10);
        model.UpdateEvent("After update", "Changed", start, start.AddHours(2));

        repository.UpdateEvent(model);
        await repository.SaveChangesAsync();

        var entity = await db.Events.SingleAsync(item => item.Id == model.Id);
        entity.Title.Should().Be("After update");
        entity.Description.Should().Be("Changed");
    }

    [Fact]
    public async Task DeleteEvent_RemovesEvent()
    {
        await fixture.ResetEventsAsync();
        await using var db = fixture.CreateEventDbContext();
        var repository = new EventRepository(db);
        var model = CreateEvent("To delete");
        await repository.CreateEventAsync(model);
        await repository.SaveChangesAsync();

        repository.DeleteEvent(model);
        await repository.SaveChangesAsync();

        (await db.Events.AnyAsync(item => item.Id == model.Id)).Should().BeFalse();
    }

    private static EventModel CreateEvent(
        string title,
        int totalSeats = 100,
        int? availableSeats = null) => new(
        Guid.NewGuid(), title, "Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2),
        totalSeats, availableSeats ?? totalSeats);
}
