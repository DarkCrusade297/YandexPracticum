namespace Booking.Application.DTO.Events;

public sealed record EventDto(Guid Id, string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats, int AvailableSeats);
