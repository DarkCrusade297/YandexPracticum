using Domain.Models;

namespace Application.Common.Interfaces
{
    public interface IEventRepository
    {
        Task<EventModel> CreateEventAsync(EventModel ev);
        Task<EventModel?> GetEventByIdAsync(Guid id);
        Task<IEnumerable<EventModel>> GetAllEventsAsync();
        void UpdateEvent(EventModel ev);
        void DeleteEvent(EventModel ev);
        Task SaveChangesAsync();
    }
}
