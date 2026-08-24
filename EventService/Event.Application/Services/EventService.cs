using Event.Application.Common.Interfaces;
using Event.Application.DTO;
using Event.Domain.Exceptions;
using Event.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Event.Application.Services;

public class EventService(IEventRepository eventRepository) : IEventService
{
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

    public async Task<EventDto> GetEventAsync(Guid id) => EventDto.FromDomain(await GetModelAsync(id));

    public async Task ReserveSeatsAsync(Guid id, int count = 1)
    {
        var ev = await GetModelAsync(id);
        ev.BookSeat(count);
        eventRepository.UpdateEvent(ev);
        await eventRepository.SaveChangesAsync();
    }

    public async Task ReleaseSeatsAsync(Guid id, int count = 1)
    {
        var ev = await GetModelAsync(id);
        ev.ReleaseSeat(count);
        eventRepository.UpdateEvent(ev);
        await eventRepository.SaveChangesAsync();
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
        return EventDto.FromDomain(model);
    }

    private async Task<EventModel> GetModelAsync(Guid id) =>
        await eventRepository.GetEventByIdAsync(id) ?? throw new NotFoundException($"Event with id '{id}' not found");
}
