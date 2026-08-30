using System.Text.Json;
using Event.Application.Common.Caching;
using Event.Application.Common.Interfaces;
using Event.Application.DTO;
using Event.Domain.Exceptions;
using Event.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Event.Application.Services;

public class EventService(
    IEventRepository eventRepository,
    ICacheService cacheService,
    IOptions<EventCacheOptions> cacheOptions,
    ILogger<EventService> logger) : IEventService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly TimeSpan _eventCacheTtl = TimeSpan.FromMinutes(cacheOptions.Value.EventTtlMinutes);

    public async Task<EventDto> CreateEventAsync(CreateEventDto dto)
    {
        var ev = new EventModel(dto.Title, dto.Description, dto.StartAt, dto.EndAt, dto.TotalSeats);
        await eventRepository.CreateEventAsync(ev);
        await eventRepository.SaveChangesAsync();
        return EventDto.FromDomain(ev);
    }

    public async Task DeleteEventAsync(Guid id)
    {
        var ev = await GetModelAsync(id);
        eventRepository.DeleteEvent(ev);
        await eventRepository.SaveChangesAsync();
        await cacheService.RemoveAsync(EventCacheKeys.ById(id));
    }

    public async Task<PaginatedResultDto> GetAllEventsAsync(string? title, DateTime? from, DateTime? to, int? page, int? pageSize)
    {
        var query = await eventRepository.GetAllEventsAsync();
        if (!string.IsNullOrWhiteSpace(title)) query = query.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        if (from.HasValue) query = query.Where(e => e.StartAt >= from.Value);
        if (to.HasValue) query = query.Where(e => e.EndAt <= to.Value);
        var currentPage = page is > 0 ? page.Value : 1;
        var size = pageSize is > 0 ? pageSize.Value : 10;
        var items = query.ToList();
        return new PaginatedResultDto
        {
            Total = items.Count,
            Events = items.OrderBy(e => e.StartAt).Skip((currentPage - 1) * size).Take(size).Select(EventDto.FromDomain).ToList(),
            CurrentPage = currentPage,
            PageSize = size
        };
    }

    public async Task<EventDto> GetEventAsync(Guid id)
    {
        var cacheKey = EventCacheKeys.ById(id);
        var cachedValue = await cacheService.GetAsync(cacheKey);
        if (cachedValue is not null)
        {
            try
            {
                var cachedEvent = JsonSerializer.Deserialize<EventDto>(cachedValue, SerializerOptions);
                if (cachedEvent is not null)
                    return cachedEvent;
            }
            catch (JsonException exception)
            {
                logger.LogWarning(exception, "Cached event {EventId} contains invalid JSON", id);
            }

            await cacheService.RemoveAsync(cacheKey);
        }

        var eventDto = EventDto.FromDomain(await GetModelAsync(id));
        await cacheService.SetAsync(
            cacheKey,
            JsonSerializer.Serialize(eventDto, SerializerOptions),
            _eventCacheTtl);
        return eventDto;
    }

    public async Task ReserveSeatsAsync(Guid id, int count = 1)
    {
        var ev = await GetModelAsync(id);
        ev.BookSeat(count);
        eventRepository.UpdateEvent(ev);
        await eventRepository.SaveChangesAsync();
        await cacheService.RemoveAsync(EventCacheKeys.ById(id));
    }

    public async Task ReleaseSeatsAsync(Guid id, int count = 1)
    {
        var ev = await GetModelAsync(id);
        ev.ReleaseSeat(count);
        eventRepository.UpdateEvent(ev);
        await eventRepository.SaveChangesAsync();
        await cacheService.RemoveAsync(EventCacheKeys.ById(id));
    }

    public async Task<EventDto> UpdateEventAsync(Guid id, UpdateEventDto dto)
    {
        var model = await GetModelAsync(id);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(dto, new ValidationContext(dto), results, true))
            throw new ValidationException(results.First().ErrorMessage);
        model.UpdateEvent(dto.Title!, dto.Description, dto.StartAt!.Value, dto.EndAt!.Value);
        eventRepository.UpdateEvent(model);
        await eventRepository.SaveChangesAsync();
        await cacheService.RemoveAsync(EventCacheKeys.ById(id));
        return EventDto.FromDomain(model);
    }

    private async Task<EventModel> GetModelAsync(Guid id) =>
        await eventRepository.GetEventByIdAsync(id) ?? throw new NotFoundException($"Event with id '{id}' not found");
}
