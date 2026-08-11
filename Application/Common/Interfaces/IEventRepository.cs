using Application.Entites;
using EventManagerSystem.DTO.Events;

namespace Application.Common.Interfaces
{
    public interface IEventRepository
    {
        Task<EventDto> CreateEventAsync(CreateEventDto eventDto);
        Task DeleteEventAsync(EventDto _event);
        Task<EventDto?> GetEventByIdAsync(Guid id);
        Task<EventDto> UpdateEventAsync(Guid id, UpdateEventDto eventDto);
        IQueryable<EventDto> GetAllEventsAsync();
        Task SaveChangesAsync();
    }
}
