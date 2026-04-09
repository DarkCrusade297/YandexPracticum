using EventManagerSystem.Models;

namespace EventManagerSystem.Services
{
    public class EventService : IEventService
    {
        public List<EventModel> Events { get; set; } = new List<EventModel>();
        public EventModel CreateEvent(EventModel eventModel)
        {
            Events.Add(eventModel);
            return Events.First(e =>  e.Id == eventModel.Id);
        }

        public bool DeleteEvent(Guid id)
        {
            if (Events.Any(e => e.Id.Equals(id)))
            {
                Events.RemoveAll(e => e.Id == id);
                return true;
            }
            return false;
        }

        public List<EventModel> GetAllEvents()
        {
            return Events;
        }

        public EventModel GetEvent(Guid id)
        {
            return Events.First(e => e.Id.Equals(id));
        }

        public EventModel UpdateEvent(Guid id, EventModel eventModel)
        {
            throw new NotImplementedException();
        }
    }
}
