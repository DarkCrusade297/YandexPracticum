using Event.Application.DTO;

namespace Event.Application.Services;

public interface IEventService
{
    Task<PaginatedResultDto> GetAllEventsAsync(string? title, DateTime? from, DateTime? to, int? page, int? pageSize);
    Task<EventDto> GetEventAsync(Guid id);
    Task<EventDto> CreateEventAsync(CreateEventDto dto);
    Task<EventDto> UpdateEventAsync(Guid id, UpdateEventDto dto);
    Task DeleteEventAsync(Guid id);
    Task ReserveSeatsAsync(Guid id, int count = 1);
    Task ReleaseSeatsAsync(Guid id, int count = 1);
}
