using Application.Common.Interfaces;
using EventManagerSystem.DataAccess;
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

        public async Task<EventModel> CreateEventAsync(EventModel @event)
        {
            await _db.Events.AddAsync(@event);
            return @event;
        }

        public void DeleteEvent(EventModel @event)
        {     
            _db.Events.Remove(@event);
        }

        public IQueryable<EventModel> GetAllEventsAsync()
        {
            var allEvents = _db.Events.AsQueryable();
            return allEvents;
        }

        public async Task<EventModel?> GetEventByIdAsync(Guid id)
        {
            var ev = await _db.Events
                        .Include(e => e.bookingModels)
                        .FirstOrDefaultAsync(e => e.Id == id);
            return ev;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void UpdateEvent(EventModel @event)
        {
            _db.Events.Update(@event);
        }
    }
}
