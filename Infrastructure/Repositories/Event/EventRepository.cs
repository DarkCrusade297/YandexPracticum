using Application.Common.Interfaces;
using Infrastructure.DataAccess;
using Domain.Exceptions;
using Domain.Models;
using Infrastructure.Mapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Event
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
            var _event = EventMapper.ToEntity(ev);
            await _db.Events.AddAsync(_event);
            return EventMapper.ToDomain(_event);
        }

        public void DeleteEvent(EventModel ev)
        {
            var entity = _db.Events.Local.FirstOrDefault(e => e.Id == ev.Id)
                 ?? _db.Events.Find(ev.Id)
                 ?? throw new NotFoundException($"Event with id: {ev.Id} not found");

            _db.Events.Remove(entity);
        }

        public async Task<IEnumerable<EventModel>> GetAllEventsAsync()
        {
            var allEvents = await _db.Events.ToListAsync();
            return allEvents.Select(e => EventMapper.ToDomain(e)).ToList();
        }

        public async Task<EventModel?> GetEventByIdAsync(Guid id)
        {
            var ev = await _db.Events
                        .Include(e => e.Bookings)
                        .FirstOrDefaultAsync(e => e.Id == id);
            if (ev is null)
                return null;
            var _event = EventMapper.ToDomain(ev);
            return _event;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void UpdateEvent(EventModel model)
        {
            var entity = _db.Events.Local.FirstOrDefault(e => e.Id == model.Id)
                         ?? _db.Events.Find(model.Id)
                         ?? throw new InvalidOperationException($"Event with id: {model.Id} not found");

            entity.AvailableSeats = model.AvailableSeats;
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.StartAt = model.StartAt;
            entity.EndAt = model.EndAt;
        }
    }
}
