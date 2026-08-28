using Event.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Event.Domain.Models;

public class EventModel
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }

    public EventModel(Guid id, string title, string? description, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats)
    {
        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = availableSeats;
    }

    public EventModel(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        if (title is null) throw new ValidationException("Title field is required");
        if (title.Length == 0) throw new ValidationException("Title cannot be empty");
        if (startAt == default) throw new ValidationException("StartAt field is required");
        if (endAt == default) throw new ValidationException("EndAt field is required");
        if (endAt <= startAt) throw new ValidationException("Дата окончания должна быть позже начала");
        if (totalSeats <= 0) throw new ValidationException("Количество мест обязательно и должно быть положительным");

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    public void BookSeat(int count)
    {
        if (count <= 0) throw new ValidationException("Count must be greater than zero");
        if (AvailableSeats < count) throw new NoAvailableSeatsException("No available seats for this event");
        AvailableSeats -= count;
    }

    public void ReleaseSeat(int count)
    {
        if (count <= 0) throw new ArgumentException("Count must be greater than zero", nameof(count));
        if (AvailableSeats + count > TotalSeats)
            throw new ArgumentException("Available seats after releasing should be less or equal then total seats", nameof(count));
        AvailableSeats += count;
    }

    public void UpdateEvent(string title, string? description, DateTime startAt, DateTime endAt)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
        if (endAt <= startAt) throw new ArgumentException("Дата окончания должна быть позже начала");
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
}
