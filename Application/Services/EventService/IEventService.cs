using Application.DTO.Events;
using EventManagerSystem.DTO.Events;
using EventManagerSystem.Models;

namespace EventManagerSystem.Services
{
    public interface IEventService
    {
        Task<PaginatedResultDto> GetAllEventsAsync(string? title, DateTime? from, DateTime? to, int? page, int? pageSize);
        Task<EventDto?> GetEventAsync(Guid id);
        Task<EventDto> CreateEventAsync(CreateEventDto dto);
        Task<EventDto> UpdateEventAsync(Guid id, UpdateEventDto dto);
        Task DeleteEventAsync(Guid id);
        Task<bool> TryReserveSeats(Guid id, int count = 1);
        Task<bool> ReleaseSeats(Guid id, int count = 1);
    }
}
