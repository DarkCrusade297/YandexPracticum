using EventManagerSystem.Models;

namespace Application.Common.Interfaces
{
    public interface IEventRepository
    {
        Task<EventModel> CreateEventAsync(EventModel @event);
        Task<EventModel?> GetEventByIdAsync(Guid id);
        IQueryable<EventModel> GetAllEventsAsync();
        void UpdateEvent(EventModel @event);
        void DeleteEvent(EventModel @event);
        Task SaveChangesAsync();
    }
}
