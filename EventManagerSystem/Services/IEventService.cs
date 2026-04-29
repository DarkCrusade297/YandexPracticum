using EventManagerSystem.DTO;
using EventManagerSystem.Models;

namespace EventManagerSystem.Services
{
    public interface IEventService
    {
        Task<PaginatedResultDto> GetAllEventsAsync(string? title, DateTime? from, DateTime? to, int? page, int? pageSize);
        Task<EventModel?> GetEventAsync(Guid id);
        Task<EventModel> CreateEventAsync(CreateEventDto dto);
        Task<EventModel> UpdateEventAsync(Guid id, UpdateEventDto dto);
        Task DeleteEventAsync(Guid id);
    }
}
