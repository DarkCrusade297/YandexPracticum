using EventManagerSystem.DataAccess;
using EventManagerSystem.DTO.Events;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using EventManagerSystem.Repositories.Event;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EventManagerSystem.Services.EventService
{
    internal class EventService : IEventService
    {
        private EventRepository _eventRepository { get; set; }
        public EventService(EventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }
        public async Task<EventModel> CreateEventAsync(CreateEventDto eventDto)
        {
            var context = new ValidationContext(eventDto);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(eventDto, context, results, validateAllProperties: true);

            if (!isValid)
                throw new ValidationException(results.First().ErrorMessage);

            return await _eventRepository.CreateEventAsync(eventDto);
        }

        public async Task DeleteEventAsync(Guid id)
        {
            await _eventRepository.DeleteEventAsync(id);
        }

        public async Task<PaginatedResultDto> GetAllEventsAsync(string? title, DateTime? from, DateTime? to, int? page, int? pageSize)
        {
            var ens = _eventRepository.GetAllEventsAsync();

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

        public async Task<EventModel?> GetEventAsync(Guid id)
        {
            return await _eventRepository.GetEventByIdAsync(id);
        }

        public async Task<bool> ReleaseSeats(Guid id, int count = 1)
        {
            return await _eventRepository.ReleaseSeats(id, count);
        }

        public async Task<bool> TryReserveSeats(Guid id, int count = 1)
        {
            return await _eventRepository.TryReserveSeats(id, count);
        }

        public async Task<EventModel> UpdateEventAsync(Guid id, UpdateEventDto eventDto)
        {
            var model = await _eventRepository.GetEventByIdAsync(id);
            if (model is null)
                throw new NotFoundException($"Event with id '{id}' not found");

            if (string.IsNullOrWhiteSpace(eventDto.Title))
                throw new ValidationException("Title is required");

            var context = new ValidationContext(eventDto);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(eventDto, context, results, validateAllProperties: true);

            if (!isValid)
                throw new ValidationException(results.First().ErrorMessage);

            return await _eventRepository.UpdateEventAsync(id, eventDto);
        }
    }
}
