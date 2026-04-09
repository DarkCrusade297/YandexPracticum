using EventManagerSystem.Models;

namespace EventManagerSystem.Services
{
    public interface IEventService
    {
        public EventModel GetEvent(Guid id);
        public List<EventModel> GetAllEvents();
        public EventModel CreateEvent(EventModel eventModel);
        public bool DeleteEvent(Guid id);
        public EventModel UpdateEvent(Guid id,  EventModel eventModel);
    }
}
