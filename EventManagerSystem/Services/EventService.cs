using EventManagerSystem.DTO;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EventManagerSystem.Services
{
    public class EventService : IEventService
    {
        public List<EventModel> Events { get; set; } = new List<EventModel>();
        public Task<EventModel> CreateEventAsync(CreateEventDto eventDto)
        {
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

        public Task<PaginatedResultDto> GetAllEventsAsync(string? title, DateTime? from, DateTime? to, int? page, int? pageSize)
        {
            var ens = Events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
               ens = ens.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (from.HasValue)
                ens = ens.Where(e => e.StartAt >= from.Value);

            if (to.HasValue)
                ens = ens.Where(e => e.EndAt <= to.Value);

            if (!page.HasValue)
                page = 1;

            if (!pageSize.HasValue)
                pageSize = 10;

            var ensCount = ens.Count();

            ens = ens.Skip(((int)page - 1) * (int)pageSize)
                .Take((int)pageSize);

            return Task.FromResult(new PaginatedResultDto { total = ensCount, events = ens.ToList(), pageSize = (int)pageSize, currentPage = (int)page });
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
