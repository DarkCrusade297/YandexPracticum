using System.Text.Json;
using Event.Application.Common.Caching;
using Event.Application.Common.Interfaces;
using Event.Application.DTO;
using Event.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ApplicationEventService = Event.Application.Services.EventService;

namespace EventService.Tests;

public sealed partial class PositiveTests
{
    [Fact]
    public async Task CreateEventAsync_ValidDto_ReturnsAndPersistsEvent()
    {
        var context = new EventServiceTestContext();
        var dto = CreateDto("Test event");

        var result = await context.Service.CreateEventAsync(dto);

        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.TotalSeats, result.TotalSeats);
        Assert.Contains(context.Repository.Events, item => item.Id == result.Id);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsAllEvents()
    {
        var context = new EventServiceTestContext(CreateModel("Event 1"), CreateModel("Event 2"), CreateModel("Event 3"));

        var result = await context.Service.GetAllEventsAsync(null, null, null, null, null);

        Assert.Equal(3, result.Total);
        Assert.Equal(3, result.Events.Count);
    }

    [Fact]
    public async Task GetEventAsync_ReturnsEventById()
    {
        var model = CreateModel("Event by id");
        var context = new EventServiceTestContext(model);

        var result = await context.Service.GetEventAsync(model.Id);

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.Title, result.Title);
    }

    [Fact]
    public async Task UpdateEventAsync_ValidDto_UpdatesEvent()
    {
        var model = CreateModel("Old title");
        var context = new EventServiceTestContext(model);
        var newStart = DateTime.UtcNow.AddDays(3);
        var newEnd = newStart.AddDays(1);

        var result = await context.Service.UpdateEventAsync(model.Id, new UpdateEventDto
        {
            Title = "Updated title",
            Description = "Updated description",
            StartAt = newStart,
            EndAt = newEnd
        });

        Assert.Equal("Updated title", result.Title);
        Assert.Equal(newStart, model.StartAt);
        Assert.Equal(1, context.Repository.SaveCalls);
    }

    [Fact]
    public async Task DeleteEventAsync_ExistingEvent_DeletesEvent()
    {
        var model = CreateModel("To delete");
        var context = new EventServiceTestContext(model);

        await context.Service.DeleteEventAsync(model.Id);

        Assert.DoesNotContain(context.Repository.Events, item => item.Id == model.Id);
        Assert.Contains(EventCacheKeys.ById(model.Id), context.Cache.RemovedKeys);
    }

    [Fact]
    public async Task GetAllEventsAsync_FiltersByTitle()
    {
        var context = new EventServiceTestContext(CreateModel("Conference"), CreateModel("Workshop"));

        var result = await context.Service.GetAllEventsAsync("FER", null, null, null, null);

        Assert.Single(result.Events);
        Assert.Equal("Conference", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_FiltersByStartDate()
    {
        var threshold = DateTime.UtcNow.AddDays(5);
        var context = new EventServiceTestContext(
            CreateModel("Before", threshold.AddDays(-2), threshold.AddDays(-1)),
            CreateModel("After", threshold.AddDays(1), threshold.AddDays(2)));

        var result = await context.Service.GetAllEventsAsync(null, threshold, null, null, null);

        Assert.Single(result.Events);
        Assert.Equal("After", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_FiltersByEndDate()
    {
        var threshold = DateTime.UtcNow.AddDays(5);
        var context = new EventServiceTestContext(
            CreateModel("Before", threshold.AddDays(-2), threshold.AddDays(-1)),
            CreateModel("After", threshold.AddDays(1), threshold.AddDays(2)));

        var result = await context.Service.GetAllEventsAsync(null, null, threshold, null, null);

        Assert.Single(result.Events);
        Assert.Equal("Before", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_AppliesPagination()
    {
        var events = Enumerable.Range(1, 8).Select(index => CreateModel($"Event {index}")).ToArray();
        var context = new EventServiceTestContext(events);

        var result = await context.Service.GetAllEventsAsync(null, null, null, 2, 3);

        Assert.Equal(8, result.Total);
        Assert.Equal(3, result.Events.Count);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(3, result.PageSize);
    }

    [Fact]
    public async Task GetAllEventsAsync_AppliesCombinedFilters()
    {
        var from = DateTime.UtcNow.AddDays(5);
        var to = DateTime.UtcNow.AddDays(10);
        var context = new EventServiceTestContext(
            CreateModel("Test matching", from.AddDays(1), to.AddDays(-1)),
            CreateModel("Other matching dates", from.AddDays(1), to.AddDays(-1)),
            CreateModel("Test outside", to.AddDays(1), to.AddDays(2)));

        var result = await context.Service.GetAllEventsAsync("test", from, to, 1, 10);

        Assert.Single(result.Events);
        Assert.Equal("Test matching", result.Events[0].Title);
    }

    internal static CreateEventDto CreateDto(string title) => new()
    {
        Title = title,
        Description = "Description",
        StartAt = DateTime.UtcNow.AddDays(1),
        EndAt = DateTime.UtcNow.AddDays(2),
        TotalSeats = 10
    };

    internal static EventModel CreateModel(
        string title,
        DateTime? startAt = null,
        DateTime? endAt = null) => new(
        title,
        "Description",
        startAt ?? DateTime.UtcNow.AddDays(1),
        endAt ?? DateTime.UtcNow.AddDays(2),
        10);
}

internal sealed class EventServiceTestContext(params EventModel[] events)
{
    public StubEventRepository Repository { get; } = new(events);
    public StubCacheService Cache { get; } = new();
    public ApplicationEventService Service => new(
        Repository,
        Cache,
        Options.Create(new EventCacheOptions { EventTtlMinutes = 5, TopEventsTtlMinutes = 5 }),
        NullLogger<ApplicationEventService>.Instance);
}

internal sealed class StubEventRepository(IEnumerable<EventModel> events) : IEventRepository
{
    public List<EventModel> Events { get; } = [.. events];
    public int SaveCalls { get; private set; }

    public Task<EventModel> CreateEventAsync(EventModel ev)
    {
        Events.Add(ev);
        return Task.FromResult(ev);
    }

    public Task<EventModel?> GetEventByIdAsync(Guid id) => Task.FromResult(Events.FirstOrDefault(item => item.Id == id));
    public Task<IEnumerable<EventModel>> GetAllEventsAsync() => Task.FromResult<IEnumerable<EventModel>>(Events.ToList());
    public Task<IReadOnlyList<EventModel>> GetTopEventsAsync(int count) =>
        Task.FromResult<IReadOnlyList<EventModel>>(Events.Take(count).ToList());
    public void UpdateEvent(EventModel ev) { }
    public void DeleteEvent(EventModel ev) => Events.Remove(ev);
    public Task SaveChangesAsync() { SaveCalls++; return Task.CompletedTask; }
}

internal sealed class StubCacheService : ICacheService
{
    public Dictionary<string, string> Values { get; } = [];
    public List<string> RemovedKeys { get; } = [];
    public Task<string?> GetAsync(string key) => Task.FromResult(Values.GetValueOrDefault(key));
    public Task SetAsync(string key, string value, TimeSpan expiration) { Values[key] = value; return Task.CompletedTask; }
    public Task RemoveAsync(string key) { Values.Remove(key); RemovedKeys.Add(key); return Task.CompletedTask; }
}

public sealed partial class PositiveTests
{
    private const int EventTtlMinutes = 3;
    private const int TopEventsTtlMinutes = 11;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GetEventAsync_CacheHit_DoesNotCallRepository()
    {
        var operations = new List<string>();
        var repository = new StubEventRepository(operations);
        var cache = new StubCacheService(operations);
        var expected = EventDto.FromDomain(CreateEvent());
        cache.Values[EventCacheKeys.ById(expected.Id)] = JsonSerializer.Serialize(expected, SerializerOptions);
        var service = CreateService(repository, cache);

        var result = await service.GetEventAsync(expected.Id);

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Title, result.Title);
        Assert.Equal(0, repository.GetByIdCalls);
        Assert.Empty(cache.Writes);
    }

    [Fact]
    public async Task GetEventAsync_CacheMiss_ReadsRepositoryAndStoresValue()
    {
        var operations = new List<string>();
        var model = CreateEvent();
        var repository = new StubEventRepository(operations) { EventById = model };
        var cache = new StubCacheService(operations);
        var service = CreateService(repository, cache);
        var cacheKey = EventCacheKeys.ById(model.Id);

        var result = await service.GetEventAsync(model.Id);

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(1, repository.GetByIdCalls);
        var write = Assert.Single(cache.Writes);
        Assert.Equal(cacheKey, write.Key);
        Assert.Equal(TimeSpan.FromMinutes(EventTtlMinutes), write.Expiration);
        Assert.Equal(model.Id, JsonSerializer.Deserialize<EventDto>(write.Value, SerializerOptions)!.Id);
        Assert.Equal([$"cache.get:{cacheKey}", "repository.get-by-id", $"cache.set:{cacheKey}"], operations);
    }

    [Fact]
    public async Task GetTopEventsAsync_CacheHit_DoesNotCallRepository()
    {
        var operations = new List<string>();
        var repository = new StubEventRepository(operations);
        var cache = new StubCacheService(operations);
        var expected = new List<TopEventDto> { TopEventDto.FromEvent(CreateEvent()) };
        cache.Values[EventCacheKeys.Top10] = JsonSerializer.Serialize(expected, SerializerOptions);
        var service = CreateService(repository, cache);

        var result = await service.GetTopEventsAsync();

        Assert.Single(result);
        Assert.Equal(expected[0].Id, result[0].Id);
        Assert.Equal(0, repository.GetTopCalls);
        Assert.Empty(cache.Writes);
    }

    [Fact]
    public async Task GetTopEventsAsync_CacheMiss_ReadsTenFromRepositoryAndStoresValue()
    {
        var operations = new List<string>();
        var model = CreateEvent();
        var repository = new StubEventRepository(operations) { TopEvents = [model] };
        var cache = new StubCacheService(operations);
        var service = CreateService(repository, cache);

        var result = await service.GetTopEventsAsync();

        Assert.Single(result);
        Assert.Equal(1, repository.GetTopCalls);
        Assert.Equal(10, repository.RequestedTopCount);
        var write = Assert.Single(cache.Writes);
        Assert.Equal(EventCacheKeys.Top10, write.Key);
        Assert.Equal(TimeSpan.FromMinutes(TopEventsTtlMinutes), write.Expiration);
        Assert.Equal(model.Id, JsonSerializer.Deserialize<List<TopEventDto>>(write.Value, SerializerOptions)![0].Id);
        Assert.Equal(
            [$"cache.get:{EventCacheKeys.Top10}", "repository.get-top", $"cache.set:{EventCacheKeys.Top10}"],
            operations);
    }

    [Theory]
    [InlineData("update")]
    [InlineData("reserve")]
    [InlineData("release")]
    public async Task Mutation_UpdatesEventCacheAfterSavingDatabase(string operation)
    {
        var operations = new List<string>();
        var model = CreateEvent(availableSeats: 5);
        var repository = new StubEventRepository(operations) { EventById = model };
        var cache = new StubCacheService(operations);
        var service = CreateService(repository, cache);

        switch (operation)
        {
            case "update":
                await service.UpdateEventAsync(model.Id, new UpdateEventDto
                {
                    Title = "Updated event",
                    Description = "Updated description",
                    StartAt = model.StartAt.AddDays(1),
                    EndAt = model.EndAt.AddDays(1)
                });
                break;
            case "reserve":
                await service.ReserveSeatsAsync(model.Id);
                break;
            case "release":
                await service.ReleaseSeatsAsync(model.Id);
                break;
        }

        var cacheKey = EventCacheKeys.ById(model.Id);
        var write = Assert.Single(cache.Writes);
        Assert.Equal(cacheKey, write.Key);
        Assert.Equal(TimeSpan.FromMinutes(EventTtlMinutes), write.Expiration);
        Assert.True(operations.IndexOf("repository.save") < operations.IndexOf($"cache.set:{cacheKey}"));
        Assert.DoesNotContain(cache.Writes, item => item.Key == EventCacheKeys.Top10);
        Assert.DoesNotContain(EventCacheKeys.Top10, cache.RemovedKeys);
    }

    [Fact]
    public async Task DeleteEventAsync_InvalidatesEventCacheAfterSavingDatabase()
    {
        var operations = new List<string>();
        var model = CreateEvent();
        var repository = new StubEventRepository(operations) { EventById = model };
        var cache = new StubCacheService(operations);
        var service = CreateService(repository, cache);
        var cacheKey = EventCacheKeys.ById(model.Id);

        await service.DeleteEventAsync(model.Id);

        Assert.Contains(cacheKey, cache.RemovedKeys);
        Assert.True(operations.IndexOf("repository.save") < operations.IndexOf($"cache.remove:{cacheKey}"));
        Assert.DoesNotContain(EventCacheKeys.Top10, cache.RemovedKeys);
        Assert.DoesNotContain(cache.Writes, item => item.Key == EventCacheKeys.Top10);
    }

    [Fact]
    public async Task CreateEventAsync_DoesNotChangeCache()
    {
        var operations = new List<string>();
        var repository = new StubEventRepository(operations);
        var cache = new StubCacheService(operations);
        var service = CreateService(repository, cache);

        await service.CreateEventAsync(new CreateEventDto
        {
            Title = "New event",
            Description = "Description",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 10
        });

        Assert.Empty(cache.Writes);
        Assert.Empty(cache.RemovedKeys);
        Assert.Contains("repository.save", operations);
    }

    private static ApplicationEventService CreateService(
        StubEventRepository repository,
        StubCacheService cache) =>
        new(
            repository,
            cache,
            Options.Create(new EventCacheOptions
            {
                EventTtlMinutes = EventTtlMinutes,
                TopEventsTtlMinutes = TopEventsTtlMinutes
            }),
            NullLogger<ApplicationEventService>.Instance);

    private static EventModel CreateEvent(int availableSeats = 10) => new(
        Guid.NewGuid(),
        "Test event",
        "Description",
        DateTime.UtcNow.AddDays(1),
        DateTime.UtcNow.AddDays(2),
        10,
        availableSeats);

    private sealed class StubCacheService(List<string> operations) : ICacheService
    {
        public Dictionary<string, string> Values { get; } = [];
        public List<CacheWrite> Writes { get; } = [];
        public List<string> RemovedKeys { get; } = [];

        public Task<string?> GetAsync(string key)
        {
            operations.Add($"cache.get:{key}");
            return Task.FromResult(Values.GetValueOrDefault(key));
        }

        public Task SetAsync(string key, string value, TimeSpan expiration)
        {
            operations.Add($"cache.set:{key}");
            Values[key] = value;
            Writes.Add(new CacheWrite(key, value, expiration));
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            operations.Add($"cache.remove:{key}");
            Values.Remove(key);
            RemovedKeys.Add(key);
            return Task.CompletedTask;
        }
    }

    private sealed class StubEventRepository(List<string> operations) : IEventRepository
    {
        public EventModel? EventById { get; init; }
        public IReadOnlyList<EventModel> TopEvents { get; init; } = [];
        public int GetByIdCalls { get; private set; }
        public int GetTopCalls { get; private set; }
        public int? RequestedTopCount { get; private set; }

        public Task<EventModel> CreateEventAsync(EventModel ev)
        {
            operations.Add("repository.create");
            return Task.FromResult(ev);
        }

        public Task<EventModel?> GetEventByIdAsync(Guid id)
        {
            operations.Add("repository.get-by-id");
            GetByIdCalls++;
            return Task.FromResult(EventById);
        }

        public Task<IEnumerable<EventModel>> GetAllEventsAsync() =>
            Task.FromResult<IEnumerable<EventModel>>([]);

        public Task<IReadOnlyList<EventModel>> GetTopEventsAsync(int count)
        {
            operations.Add("repository.get-top");
            GetTopCalls++;
            RequestedTopCount = count;
            return Task.FromResult(TopEvents);
        }

        public void UpdateEvent(EventModel ev) => operations.Add("repository.update");

        public void DeleteEvent(EventModel ev) => operations.Add("repository.delete");

        public Task SaveChangesAsync()
        {
            operations.Add("repository.save");
            return Task.CompletedTask;
        }
    }

    private sealed record CacheWrite(string Key, string Value, TimeSpan Expiration);
}
