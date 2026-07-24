using EventManagerSystem.DataAccess;
using EventManagerSystem.DTO.Events;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EventManagerSystem.Services.EventService
{
    internal class EventService : IEventService
    {
        private AppDbContext _context { get; set; }
        public EventService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<EventModel> CreateEventAsync(CreateEventDto eventDto)
        {
            var context = new ValidationContext(eventDto);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(eventDto, context, results, validateAllProperties: true);

            if (!isValid)
                throw new ValidationException(results.First().ErrorMessage);

            var eventModel = new EventModel(eventDto.Title,
                eventDto.Description,
                eventDto.StartAt,
                eventDto.EndAt,
                eventDto.TotalSeats);
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();
            return eventModel;
        }

        public async Task DeleteEventAsync(Guid id)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (ev is null)
                throw new NotFoundException($"Event with id '{id}' not found");
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResultDto> GetAllEventsAsync(string? title, DateTime? from, DateTime? to, int? page, int? pageSize)
        {
            var ens = _context.Events.AsQueryable();

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

            return new PaginatedResultDto { total = ensCount, events = ens.ToList(), pageSize = (int)pageSize, currentPage = (int)page };
        }

        public async Task<EventModel> GetEventAsync(Guid id)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id.Equals(id));
            if (ev == null)
                throw new NotFoundException($"Event with id {id} not found");
            return ev;
        }

        public bool ReleaseSeats(Guid id, int count = 1)
        {
            var ev = _context.Events.FirstOrDefault(e => e.Id.Equals(id));
            if (ev == null)
                return false;
            if (ev.AvailableSeats == ev.TotalSeats)
                return false;
            ev.AvailableSeats += count;
            _context.SaveChangesAsync();
            return true;
        }

        public bool TryReserveSeats(Guid id, int count = 1)
        {
            var ev = _context.Events.FirstOrDefault(e => e.Id.Equals(id));
            if (ev == null)
                return false;
            if (ev.AvailableSeats < 1)
                return false;
            ev.AvailableSeats -= count;
            _context.SaveChangesAsync();
            return true;
        }

        public async Task<EventModel> UpdateEventAsync(Guid id, UpdateEventDto eventDto)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model is null)
                throw new NotFoundException($"Event with id '{id}' not found");

            if (string.IsNullOrWhiteSpace(eventDto.Title))
                throw new ValidationException("Title is required");

            var context = new ValidationContext(eventDto);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(eventDto, context, results, validateAllProperties: true);

            if (!isValid)
                throw new ValidationException(results.First().ErrorMessage);

            model.Title = eventDto.Title;
            model.Description = eventDto.Description;
            model.StartAt = eventDto.StartAt;
            model.EndAt = eventDto.EndAt;
            await _context.SaveChangesAsync();
            return model;
        }


    }
}
