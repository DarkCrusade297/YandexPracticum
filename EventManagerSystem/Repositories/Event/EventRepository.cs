using EventManagerSystem.DataAccess;
using EventManagerSystem.DTO.Events;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagerSystem.Repositories.Event
{
    internal class EventRepository : IEventRepository
    {
        private readonly AppDbContext _db;

        public EventRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<EventModel> CreateEventAsync(CreateEventDto eventDto)
        {
            var eventModel = new EventModel(eventDto.Title,
                eventDto.Description,
                eventDto.StartAt,
                eventDto.EndAt,
                eventDto.TotalSeats);
            _db.Events.Add(eventModel);
            await _db.SaveChangesAsync();
            return eventModel;
        }

        public async Task DeleteEventAsync(Guid id)
        {
            var ev = _db.Events.FirstOrDefault(e => e.Id == id);
            if (ev is null)
                throw new NotFoundException($"Event with id '{id}' not found");        
            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
        }

        public IQueryable<EventModel> GetAllEventsAsync()
        {
            var allEvents = _db.Events.AsQueryable();
            return allEvents;
        }

        public async Task<EventModel> GetEventByIdAsync(Guid id)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (ev is null)
                throw new NotFoundException($"Event with id '{id}' not found");
            return ev;
        }

        public async Task<bool> ReleaseSeats(Guid id, int count = 1)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null)
                return false;
            if (ev.AvailableSeats == ev.TotalSeats)
                return false;
            ev.AvailableSeats += count;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TryReserveSeats(Guid id, int count = 1)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null)
                return false;
            if (ev.AvailableSeats < 1)
                return false;
            ev.AvailableSeats -= count;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<EventModel> UpdateEventAsync(Guid id, UpdateEventDto eventDto)
        {
            var model = await GetEventByIdAsync(id);
            model.Title = eventDto.Title;
            model.Description = eventDto.Description;
            model.StartAt = eventDto.StartAt;
            model.EndAt = eventDto.EndAt;
            await _db.SaveChangesAsync();
            return model;
        }
    }
}
