using EventManagerSystem.DTO.Events;
using EventManagerSystem.Models;

namespace EventManagerSystem.Repositories.Event
{
    public interface IEventRepository
    {
        Task<EventModel> CreateEventAsync(CreateEventDto eventDto);
        Task DeleteEventAsync(EventModel _event);
        Task<EventModel?> GetEventByIdAsync(Guid id);
        Task<EventModel> UpdateEventAsync(Guid id, UpdateEventDto eventDto);
        IQueryable<EventModel> GetAllEventsAsync();
        Task SaveChangesAsync();
    }
}
