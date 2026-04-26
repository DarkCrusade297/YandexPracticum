using EventManagerSystem.DTO;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;

namespace EventManagerSystem.Services
{
    public class EventService : IEventService
    {
        public List<EventModel> Events { get; set; } = new List<EventModel>();
        public EventModel CreateEvent(CreateEventDto eventDto)
        {
                EventModel eventModel = new EventModel(eventDto.Title,
                eventDto.Description,
                eventDto.StartAt,
                eventDto.EndAt);
                Events.Add(eventModel);
                return eventModel;
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
            return Events.FirstOrDefault(e => e.Id.Equals(id));
        }

        public EventModel UpdateEvent(Guid id, UpdateEventDto eventDto)
        {         
            EventModel model = Events.FirstOrDefault(e => e.Id.Equals(id));
            if (model == null)
            {
                throw new NotFoundException($"Not found Event with id: {id}");
            }
            model.Title = eventDto.Title;
            model.Description = eventDto.Description;
            model.StartAt = eventDto.StartAt;
            model.EndAt = eventDto.EndAt;
            return model;
        }
    }
}
