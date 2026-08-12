using Application.Common.Interfaces;
using Application.DTO.Events;
using EventManagerSystem.DTO.Events;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace EventManagerSystem.Services.EventService
{
    internal class EventService : IEventService
    {
        private IEventRepository _eventRepository { get; set; }
        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }
        public async Task<EventDto> CreateEventAsync(CreateEventDto dto)
        {
            var ev = new EventModel(dto.Title, dto.Description, dto.StartAt, dto.EndAt, dto.TotalSeats);
            await _eventRepository.CreateEventAsync(ev);
            await _eventRepository.SaveChangesAsync();
            return EventDto.FromDomain(ev);
        }

        public async Task DeleteEventAsync(Guid id)
        {
            var ev = await _eventRepository.GetEventByIdAsync(id);
            if (ev is null)
                throw new NotFoundException($"Event with id '{id}' not found");
            _eventRepository.DeleteEvent(ev);
            await _eventRepository.SaveChangesAsync();
        }

        public async Task<PaginatedResultDto> GetAllEventsAsync(string? title, DateTime? from, DateTime? to, int? page, int? pageSize)
        {
            var query = _eventRepository.GetAllEventsAsync();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(e => e.Title.Contains(title));

            if (from.HasValue)
                query = query.Where(e => e.StartAt >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.EndAt <= to.Value);

            var currentPage = page is > 0 ? page.Value : 1;
            var size = pageSize is > 0 ? pageSize.Value : 10;

            var total = query.Count();

            var items = query
                .OrderBy(e => e.StartAt)
                .Skip((currentPage - 1) * size)
                .Take(size)
                .ToList();

            return new PaginatedResultDto
            {
                total = total,
                events = items.Select(EventDto.FromDomain).ToList(),
                pageSize = size,
                currentPage = currentPage
            };
        }

        public async Task<EventDto?> GetEventAsync(Guid id)
        {
            var ev = await _eventRepository.GetEventByIdAsync(id);
            if (ev is null)
                throw new NotFoundException($"Event with id '{id}' not found");
            return EventDto.FromDomain(ev);
        }

        public async Task<bool> ReleaseSeats(Guid id, int count = 1)
        {
            var ev = await _eventRepository.GetEventByIdAsync(id);
            if (ev == null)
                return false;
            for (var i = 0; i < count; i++)
                ev.ReleaseSeat();
            await _eventRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TryReserveSeats(Guid id, int count = 1)
        {
            var ev = await _eventRepository.GetEventByIdAsync(id);
            if (ev == null)
                return false;
            if (ev.AvailableSeats < count)
                return false;
            try
            {
                for (var i = 0; i < count; i++)
                    ev.BookSeat();
            }
            catch (NoAvailableSeatsException)
            {
                return false;
            }
            await _eventRepository.SaveChangesAsync();
            return true;
        }

        public async Task<EventDto> UpdateEventAsync(Guid id, UpdateEventDto eventDto)
        {
            var model = await _eventRepository.GetEventByIdAsync(id);
            if (model is null)
                throw new NotFoundException($"Event with id '{id}' not found");
            ValidateDto(eventDto);
            model.UpdateEvent(
                eventDto.Title!,            
                eventDto.Description,
                eventDto.StartAt!.Value,
                eventDto.EndAt!.Value);
            _eventRepository.UpdateEvent(model);
            await _eventRepository.SaveChangesAsync();
            return EventDto.FromDomain(model);
        }

        private static void ValidateDto(UpdateEventDto eventDto)
        {
            if (string.IsNullOrWhiteSpace(eventDto.Title))
                throw new ValidationException("Title is required");

            var context = new ValidationContext(eventDto);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(eventDto, context, results, validateAllProperties: true))
                throw new ValidationException(results.First().ErrorMessage);
        }
    }
}
