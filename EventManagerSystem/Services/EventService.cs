using EventManagerSystem.DTO;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace EventManagerSystem.Services
{
    public class EventService : IEventService
    {
        public List<EventModel> Events { get; set; } = new List<EventModel>();
        public Task<EventModel> CreateEventAsync(CreateEventDto eventDto)
        {
            if (string.IsNullOrWhiteSpace(eventDto.Title))
                throw new ValidationException("Title is required");

            if (Events.Any(e => e.Title == eventDto.Title))
                throw new ConflictException($"Event with title '{eventDto.Title}' already exists");

            var eventModel = new EventModel(eventDto.Title,
                eventDto.Description,
                eventDto.StartAt,
                eventDto.EndAt);

            Events.Add(eventModel);
            return Task.FromResult(eventModel);
        }

        public Task DeleteEventAsync(Guid id)
        {
            var ev = Events.FirstOrDefault(e => e.Id == id);
            if (ev is null)
                throw new NotFoundException($"Event with id '{id}' not found");

            Events.Remove(ev);
            return Task.CompletedTask;
        }

        public Task<List<EventModel>> GetAllEventsAsync()
        {
            return Task.FromResult(Events);
        }

        public Task<EventModel> GetEventAsync(Guid id)
        {
            var ev = Events.FirstOrDefault(e => e.Id.Equals(id));
            if (ev == null)
                throw new NotFoundException($"Event with id {id} not found");
            return Task.FromResult(ev);
        }

        public Task<EventModel> UpdateEventAsync(Guid id, UpdateEventDto eventDto)
        {
            var model = Events.FirstOrDefault(e => e.Id == id);
            if (model is null)
                throw new NotFoundException($"Event with id '{id}' not found");

            if (string.IsNullOrWhiteSpace(eventDto.Title))
                throw new ValidationException("Title is required");

            model.Title = eventDto.Title;
            model.Description = eventDto.Description;
            model.StartAt = eventDto.StartAt;
            model.EndAt = eventDto.EndAt;

            return Task.FromResult(model);
        }
    }
}
