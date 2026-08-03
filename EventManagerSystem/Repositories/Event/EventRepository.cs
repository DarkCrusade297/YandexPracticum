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

        public async Task DeleteEventAsync(EventModel _event)
        {     
            _db.Events.Remove(_event);
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
            return ev;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
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
