using EventManagerSystem.DTO;
using EventManagerSystem.Models;
using Microsoft.Extensions.Logging;

namespace EventService.Tests;

public class PositiveTests
{
    private readonly EventManagerSystem.Services.EventService _sut = new EventManagerSystem.Services.EventService();

    [Fact]
    public async Task CreateEventAsync_ValidDto_ReturnsCreatedEvent()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.UtcNow.AddDays(1).Date,
            EndAt = DateTime.UtcNow.AddDays(2).Date
        };

        // Act
        var result = await _sut.CreateEventAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.StartAt, result.StartAt);
        Assert.Equal(dto.EndAt, result.EndAt);
        Assert.Contains(result, _sut.Events);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsAllEvents()
    {
        // Arrange
        _sut.Events.AddRange(new[]
        {
        new EventModel("Event 1", "Description 1", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date),
        new EventModel("Event 2", "Description 2", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date),
        new EventModel("Event 3", "Description 3", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date),
    });

        // Act
        var result = await _sut.GetAllEventsAsync(null, null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.total);
        Assert.Equal(3, result.events.Count);
    }

    [Fact]
    public async Task GetEventAsync_ReturnEventById()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.UtcNow.AddDays(1).Date,
            EndAt = DateTime.UtcNow.AddDays(2).Date
        };
        var createdEvent = await _sut.CreateEventAsync(dto);

        // Act
        var _event = await _sut.GetEventAsync(createdEvent.Id);

        // Assert
        Assert.NotNull(_event);
        Assert.Equal(_event.Id, createdEvent.Id);
        Assert.Equal(_event.Title, createdEvent.Title);
        Assert.Equal(_event.Description, createdEvent.Description);
        Assert.Equal(_event.StartAt, createdEvent.StartAt);
        Assert.Equal(_event.EndAt, createdEvent.EndAt);
    }

    [Fact]
    public async Task UpdateEventAsync_ValidId_UpdateEventById()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.UtcNow.AddDays(1).Date,
            EndAt = DateTime.UtcNow.AddDays(2).Date
        };
        var createdEvent = await _sut.CreateEventAsync(dto);

        // Act
        await _sut.UpdateEventAsync(createdEvent.Id, new UpdateEventDto
        {
            Title = "UpdatedTitle",
            Description = "UpdatedDescription",
            StartAt = DateTime.UtcNow.AddDays(3).Date,
            EndAt = DateTime.UtcNow.AddDays(4).Date
        });
        var updatedEvent = await _sut.GetEventAsync(createdEvent.Id);

        // Assert
        Assert.NotNull(updatedEvent);
        Assert.Equal(updatedEvent.Id, createdEvent.Id);
        Assert.Equal("UpdatedTitle", updatedEvent.Title);
        Assert.Equal("UpdatedDescription", updatedEvent.Description);
        Assert.Equal(updatedEvent.StartAt, DateTime.UtcNow.AddDays(3).Date);
        Assert.Equal(updatedEvent.EndAt, DateTime.UtcNow.AddDays(4).Date);
    }

    [Fact]
    public async Task DeleteEventAsync_DeleteEventById()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.UtcNow.AddDays(1).Date,
            EndAt = DateTime.UtcNow.AddDays(2).Date
        };
        var createdEvent = await _sut.CreateEventAsync(dto);

        // Act
        await _sut.DeleteEventAsync(createdEvent.Id);

        // Assert
        Assert.Empty(_sut.Events);
        Assert.DoesNotContain(createdEvent, _sut.Events);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsFilteredByTitleEvents()
    {
        // Arrange
        _sut.Events.AddRange(new[]
        {
        new EventModel("Event 1", "Description 1", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date),
        new EventModel("Test", "Description 2", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date),
        new EventModel("Test 2", "Description 3", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date),
        });

        // Act
        var result = await _sut.GetAllEventsAsync("EV", null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.total);
        Assert.Single(result.events);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsFilteredByStartAtEvents()
    {
        // Arrange
        _sut.Events.AddRange(new[]
        {
        new EventModel("Test 0", "Description 0", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date),
        new EventModel("Test 1", "Description 1", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date),
        new EventModel("Test 2", "Description 2", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date),
        new EventModel("Test 3", "Description 3", DateTime.UtcNow.AddDays(7).Date, DateTime.UtcNow.AddDays(8).Date),
        new EventModel("Test 4", "Description 4", DateTime.UtcNow.AddDays(9).Date, DateTime.UtcNow.AddDays(10).Date),
        new EventModel("Test 5", "Description 5", DateTime.UtcNow.AddDays(11).Date, DateTime.UtcNow.AddDays(12).Date),
        new EventModel("Test 6", "Description 6", DateTime.UtcNow.AddDays(13).Date, DateTime.UtcNow.AddDays(14).Date),
        new EventModel("Test 7", "Description 7", DateTime.UtcNow.AddDays(15).Date, DateTime.UtcNow.AddDays(16).Date),
        });

        // Act
        var result = await _sut.GetAllEventsAsync(null, DateTime.UtcNow.AddDays(5).Date, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.total);
        Assert.Equal(6, result.events.Count);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsFilteredByEndAtEvents()
    {
        // Arrange
        _sut.Events.AddRange(new[]
        {
        new EventModel("Test 0", "Description 0", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date),
        new EventModel("Test 1", "Description 1", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date),
        new EventModel("Test 2", "Description 2", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date),
        new EventModel("Test 3", "Description 3", DateTime.UtcNow.AddDays(7).Date, DateTime.UtcNow.AddDays(8).Date),
        new EventModel("Test 4", "Description 4", DateTime.UtcNow.AddDays(9).Date, DateTime.UtcNow.AddDays(10).Date),
        new EventModel("Test 5", "Description 5", DateTime.UtcNow.AddDays(11).Date, DateTime.UtcNow.AddDays(12).Date),
        new EventModel("Test 6", "Description 6", DateTime.UtcNow.AddDays(13).Date, DateTime.UtcNow.AddDays(14).Date),
        new EventModel("Test 7", "Description 7", DateTime.UtcNow.AddDays(15).Date, DateTime.UtcNow.AddDays(16).Date),
        });

        // Act
        var result = await _sut.GetAllEventsAsync(null, null, DateTime.UtcNow.AddDays(6).Date, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.total);
        Assert.Equal(3, result.events.Count);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsFilteredByPaginationEvents()
    {
        // Arrange
        _sut.Events.AddRange(new[]
        {
        new EventModel("Test 0", "Description 0", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date),
        new EventModel("Test 1", "Description 1", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date),
        new EventModel("Test 2", "Description 2", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date),
        new EventModel("Test 3", "Description 3", DateTime.UtcNow.AddDays(7).Date, DateTime.UtcNow.AddDays(8).Date),
        new EventModel("Test 4", "Description 4", DateTime.UtcNow.AddDays(9).Date, DateTime.UtcNow.AddDays(10).Date),
        new EventModel("Test 5", "Description 5", DateTime.UtcNow.AddDays(11).Date, DateTime.UtcNow.AddDays(12).Date),
        new EventModel("Test 6", "Description 6", DateTime.UtcNow.AddDays(13).Date, DateTime.UtcNow.AddDays(14).Date),
        new EventModel("Test 7", "Description 7", DateTime.UtcNow.AddDays(15).Date, DateTime.UtcNow.AddDays(16).Date),
        });

        // Act
        var result = await _sut.GetAllEventsAsync(null, null, null, 2, 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(8, result.total);
        Assert.Equal(3, result.events.Count);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsByCombinatedFilteredEvents()
    {
        // Arrange
        _sut.Events.AddRange(new[]
        {
        new EventModel("Test 0", "Description 0", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date),
        new EventModel("Test 1", "Description 1", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date),
        new EventModel("Test 2", "Description 2", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date),
        new EventModel("Test 3", "Description 3", DateTime.UtcNow.AddDays(7).Date, DateTime.UtcNow.AddDays(8).Date),
        new EventModel("Event 4", "Description 4", DateTime.UtcNow.AddDays(9).Date, DateTime.UtcNow.AddDays(10).Date),
        new EventModel("Event 5", "Description 5", DateTime.UtcNow.AddDays(11).Date, DateTime.UtcNow.AddDays(12).Date),
        new EventModel("Event 6", "Description 6", DateTime.UtcNow.AddDays(13).Date, DateTime.UtcNow.AddDays(14).Date),
        new EventModel("Event 7", "Description 7", DateTime.UtcNow.AddDays(15).Date, DateTime.UtcNow.AddDays(16).Date),
        });

        // Act
        var result = await _sut.GetAllEventsAsync("ES", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(8).Date, 1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.total);
        Assert.Equal(1, result.events.Count);
    }
}
