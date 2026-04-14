using EventManagerSystem.DTO;
using EventManagerSystem.Models;

namespace EventManagerSystem.Services
{
    public interface IEventService
    {
        public EventModel GetEvent(Guid id);
        public List<EventModel> GetAllEvents();
        public EventModel CreateEvent(CreateEventDto dto);
        public bool DeleteEvent(Guid id);
        public EventModel UpdateEvent(Guid id,  UpdateEventDto eventModel);
    }
}
