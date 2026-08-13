using Application.Common.Interfaces;
using EventManagerSystem.DataAccess;
using EventManagerSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagerSystem.Repositories.Event
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _db;

        public EventRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<EventModel> CreateEventAsync(EventModel ev)
        {
            await _db.Events.AddAsync(ev);
            return ev;
        }

        public void DeleteEvent(EventModel ev)
        {     
            _db.Events.Remove(ev);
        }

        public IQueryable<EventModel> GetAllEventsAsync()
        {
            var allEvents = _db.Events.AsQueryable();
            return allEvents;
        }

        public async Task<EventModel?> GetEventByIdAsync(Guid id)
        {
            var ev = await _db.Events
                        .Include(e => e.Bookings)
                        .FirstOrDefaultAsync(e => e.Id == id);
            return ev;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void UpdateEvent(EventModel ev)
        {
            _db.Events.Update(ev);
        }
    }
}
