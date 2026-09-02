using Event.Application.Common.Interfaces;
using Event.Domain.Exceptions;
using Event.Domain.Models;
using Event.Infrastructure.DataAccess;
using Event.Infrastructure.Mapper;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Repositories;

public class EventRepository(EventDbContext db) : IEventRepository
{
    public async Task<EventModel> CreateEventAsync(EventModel ev)
    {
        await db.Events.AddAsync(EventMapper.ToEntity(ev));
        return ev;
    }

    public async Task<EventModel?> GetEventByIdAsync(Guid id)
    {
        var entity = await db.Events.FirstOrDefaultAsync(e => e.Id == id);
        return entity is null ? null : EventMapper.ToDomain(entity);
    }

    public async Task<IEnumerable<EventModel>> GetAllEventsAsync() =>
        (await db.Events.AsNoTracking().ToListAsync()).Select(EventMapper.ToDomain).ToList();

    public async Task<IReadOnlyList<EventModel>> GetTopEventsAsync(int count) =>
        (await db.Events
            .AsNoTracking()
            .OrderByDescending(e => e.TotalSeats > 0
                ? (double)(e.TotalSeats - e.AvailableSeats) / e.TotalSeats
                : 0d)
            .ThenBy(e => e.Id)
            .Take(count)
            .ToListAsync())
        .Select(EventMapper.ToDomain)
        .ToList();

    public void UpdateEvent(EventModel model)
    {
        var entity = db.Events.Local.FirstOrDefault(e => e.Id == model.Id)
            ?? db.Events.Find(model.Id)
            ?? throw new NotFoundException($"Event with id: {model.Id} not found");
        entity.Title = model.Title;
        entity.Description = model.Description;
        entity.StartAt = model.StartAt;
        entity.EndAt = model.EndAt;
        entity.AvailableSeats = model.AvailableSeats;
    }

    public void DeleteEvent(EventModel model)
    {
        var entity = db.Events.Local.FirstOrDefault(e => e.Id == model.Id)
            ?? db.Events.Find(model.Id)
            ?? throw new NotFoundException($"Event with id: {model.Id} not found");
        db.Events.Remove(entity);
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
