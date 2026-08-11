using EventManagerSystem.DataAccess;
using EventManagerSystem.DTO.Events;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using EventManagerSystem.Services;
using EventManagerSystem.Repositories.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Interfaces;

namespace EventService.Tests;

public class PositiveTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IEventService _eventService;
    private readonly AppDbContext _context;

    public PositiveTests()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventService, EventManagerSystem.Services.EventService.EventService>();

        _serviceProvider = services.BuildServiceProvider();

        _scope = _serviceProvider.CreateScope();

        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [Fact]
    public async Task CreateEventAsync_ValidDto_ReturnsCreatedEvent()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.UtcNow.AddDays(1).Date,
            EndAt = DateTime.UtcNow.AddDays(2).Date,
            TotalSeats = 10
        };

        // Act
        var result = await _eventService.CreateEventAsync(dto);

        var eventFromDb = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == result.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.StartAt, result.StartAt);
        Assert.Equal(dto.EndAt, result.EndAt);
        Assert.Equal(dto.TotalSeats, result.TotalSeats);

        Assert.NotNull(eventFromDb);
        Assert.Equal(result.Id, eventFromDb.Id);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsAllEvents()
    {
        // Arrange
        await SeedEventsAsync(
            new EventModel("Event 1", "Description 1", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date, 10),
            new EventModel("Event 2", "Description 2", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date, 50),
            new EventModel("Event 3", "Description 3", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date, 100)
        );

        // Act
        var result = await _eventService.GetAllEventsAsync(null, null, null, null, null);

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
            EndAt = DateTime.UtcNow.AddDays(2).Date,
            TotalSeats = 10
        };

        var createdEvent = await _eventService.CreateEventAsync(dto);

        // Act
        var eventDto = await _eventService.GetEventAsync(createdEvent.Id);

        // Assert
        Assert.NotNull(eventDto);
        Assert.Equal(createdEvent.Id, eventDto.Id);
        Assert.Equal(createdEvent.Title, eventDto.Title);
        Assert.Equal(createdEvent.Description, eventDto.Description);
        Assert.Equal(createdEvent.StartAt, eventDto.StartAt);
        Assert.Equal(createdEvent.EndAt, eventDto.EndAt);
        Assert.Equal(createdEvent.TotalSeats, eventDto.TotalSeats);
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
            EndAt = DateTime.UtcNow.AddDays(2).Date,
            TotalSeats = 10
        };

        var createdEvent = await _eventService.CreateEventAsync(dto);

        var expectedStartAt = DateTime.UtcNow.AddDays(3).Date;
        var expectedEndAt = DateTime.UtcNow.AddDays(4).Date;

        // Act
        await _eventService.UpdateEventAsync(createdEvent.Id, new UpdateEventDto
        {
            Title = "UpdatedTitle",
            Description = "UpdatedDescription",
            StartAt = expectedStartAt,
            EndAt = expectedEndAt
        });

        var updatedEvent = await _eventService.GetEventAsync(createdEvent.Id);

        // Assert
        Assert.NotNull(updatedEvent);
        Assert.Equal(createdEvent.Id, updatedEvent.Id);
        Assert.Equal("UpdatedTitle", updatedEvent.Title);
        Assert.Equal("UpdatedDescription", updatedEvent.Description);
        Assert.Equal(expectedStartAt, updatedEvent.StartAt);
        Assert.Equal(expectedEndAt, updatedEvent.EndAt);
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
            EndAt = DateTime.UtcNow.AddDays(2).Date,
            TotalSeats = 10
        };

        var createdEvent = await _eventService.CreateEventAsync(dto);

        // Act
        await _eventService.DeleteEventAsync(createdEvent.Id);

        var eventsCount = await _context.Events.CountAsync();

        // Assert
        Assert.Equal(0, eventsCount);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _eventService.GetEventAsync(createdEvent.Id));
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsFilteredByTitleEvents()
    {
        // Arrange
        await SeedEventsAsync(
            new EventModel("Event 1", "Description 1", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date, 10),
            new EventModel("Test", "Description 2", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date, 20),
            new EventModel("Test 2", "Description 3", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date, 30)
        );

        // Act
        var result = await _eventService.GetAllEventsAsync("EV", null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.total);
        Assert.Single(result.events);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsFilteredByStartAtEvents()
    {
        // Arrange
        await SeedEventsAsync(
            new EventModel("Test 0", "Description 0", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date, 10),
            new EventModel("Test 1", "Description 1", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date, 10),
            new EventModel("Test 2", "Description 2", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date, 10),
            new EventModel("Test 3", "Description 3", DateTime.UtcNow.AddDays(7).Date, DateTime.UtcNow.AddDays(8).Date, 10),
            new EventModel("Test 4", "Description 4", DateTime.UtcNow.AddDays(9).Date, DateTime.UtcNow.AddDays(10).Date, 10),
            new EventModel("Test 5", "Description 5", DateTime.UtcNow.AddDays(11).Date, DateTime.UtcNow.AddDays(12).Date, 10),
            new EventModel("Test 6", "Description 6", DateTime.UtcNow.AddDays(13).Date, DateTime.UtcNow.AddDays(14).Date, 10),
            new EventModel("Test 7", "Description 7", DateTime.UtcNow.AddDays(15).Date, DateTime.UtcNow.AddDays(16).Date, 10)
        );

        var startAtFilter = DateTime.UtcNow.AddDays(5).Date;

        // Act
        var result = await _eventService.GetAllEventsAsync(null, startAtFilter, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.total);
        Assert.Equal(6, result.events.Count);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsFilteredByEndAtEvents()
    {
        // Arrange
        await SeedEventsAsync(
            new EventModel("Test 0", "Description 0", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date, 10),
            new EventModel("Test 1", "Description 1", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date, 10),
            new EventModel("Test 2", "Description 2", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date, 10),
            new EventModel("Test 3", "Description 3", DateTime.UtcNow.AddDays(7).Date, DateTime.UtcNow.AddDays(8).Date, 10),
            new EventModel("Test 4", "Description 4", DateTime.UtcNow.AddDays(9).Date, DateTime.UtcNow.AddDays(10).Date, 10),
            new EventModel("Test 5", "Description 5", DateTime.UtcNow.AddDays(11).Date, DateTime.UtcNow.AddDays(12).Date, 10),
            new EventModel("Test 6", "Description 6", DateTime.UtcNow.AddDays(13).Date, DateTime.UtcNow.AddDays(14).Date, 10),
            new EventModel("Test 7", "Description 7", DateTime.UtcNow.AddDays(15).Date, DateTime.UtcNow.AddDays(16).Date, 10)
        );

        var endAtFilter = DateTime.UtcNow.AddDays(6).Date;

        // Act
        var result = await _eventService.GetAllEventsAsync(null, null, endAtFilter, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.total);
        Assert.Equal(3, result.events.Count);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsFilteredByPaginationEvents()
    {
        // Arrange
        await SeedEightEventsAsync();

        // Act
        var result = await _eventService.GetAllEventsAsync(null, null, null, 2, 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(8, result.total);
        Assert.Equal(3, result.events.Count);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsByCombinatedFilteredEvents()
    {
        // Arrange
        await SeedEventsAsync(
            new EventModel("Test 0", "Description 0", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date, 10),
            new EventModel("Test 1", "Description 1", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date, 10),
            new EventModel("Test 2", "Description 2", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date, 10),
            new EventModel("Test 3", "Description 3", DateTime.UtcNow.AddDays(7).Date, DateTime.UtcNow.AddDays(8).Date, 10),
            new EventModel("Event 4", "Description 4", DateTime.UtcNow.AddDays(9).Date, DateTime.UtcNow.AddDays(10).Date, 10),
            new EventModel("Event 5", "Description 5", DateTime.UtcNow.AddDays(11).Date, DateTime.UtcNow.AddDays(12).Date, 10),
            new EventModel("Event 6", "Description 6", DateTime.UtcNow.AddDays(13).Date, DateTime.UtcNow.AddDays(14).Date, 10),
            new EventModel("Event 7", "Description 7", DateTime.UtcNow.AddDays(15).Date, DateTime.UtcNow.AddDays(16).Date, 10)
        );

        var startAtFilter = DateTime.UtcNow.AddDays(5).Date;
        var endAtFilter = DateTime.UtcNow.AddDays(8).Date;

        // Act
        var result = await _eventService.GetAllEventsAsync("ES", startAtFilter, endAtFilter, 1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.total);
        Assert.Single(result.events);
    }

    private async Task SeedEventsAsync(params EventModel[] events)
    {
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();
    }

    private async Task SeedEightEventsAsync()
    {
        await SeedEventsAsync(
            new EventModel("Test 0", "Description 0", DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(2).Date, 10),
            new EventModel("Test 1", "Description 1", DateTime.UtcNow.AddDays(3).Date, DateTime.UtcNow.AddDays(4).Date, 10),
            new EventModel("Test 2", "Description 2", DateTime.UtcNow.AddDays(5).Date, DateTime.UtcNow.AddDays(6).Date, 10),
            new EventModel("Test 3", "Description 3", DateTime.UtcNow.AddDays(7).Date, DateTime.UtcNow.AddDays(8).Date, 10),
            new EventModel("Test 4", "Description 4", DateTime.UtcNow.AddDays(9).Date, DateTime.UtcNow.AddDays(10).Date, 10),
            new EventModel("Test 5", "Description 5", DateTime.UtcNow.AddDays(11).Date, DateTime.UtcNow.AddDays(12).Date, 10),
            new EventModel("Test 6", "Description 6", DateTime.UtcNow.AddDays(13).Date, DateTime.UtcNow.AddDays(14).Date, 10),
            new EventModel("Test 7", "Description 7", DateTime.UtcNow.AddDays(15).Date, DateTime.UtcNow.AddDays(16).Date, 10)
        );
    }

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }
}
