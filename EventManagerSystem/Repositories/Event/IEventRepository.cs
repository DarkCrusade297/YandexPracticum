using EventManagerSystem.DTO.Events;
using EventManagerSystem.Models;

namespace EventManagerSystem.Repositories.Event
{
    public interface IEventRepository
    {
        Task<EventModel> CreateEventAsync(CreateEventDto eventDto);
        Task DeleteEventAsync(Guid id);
        Task<EventModel> GetEventByIdAsync(Guid id);
        Task<bool> ReleaseSeats(Guid id, int count = 1);
        Task<bool> TryReserveSeats(Guid id, int count = 1);
        Task<EventModel> UpdateEventAsync(Guid id, UpdateEventDto eventDto);
        IQueryable<EventModel> GetAllEventsAsync();
    }
}
