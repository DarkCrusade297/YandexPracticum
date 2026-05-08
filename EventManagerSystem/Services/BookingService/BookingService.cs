using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using Microsoft.Extensions.Logging;

namespace EventManagerSystem.Services.BookingService
{
    public class BookingService : IBookingService
    {
        public List<BookingModel> Bookings { get; set; } = new List<BookingModel>();
        private IEventService eventService;

        public BookingService(IEventService eventService)
        {
            this.eventService = eventService;
        }

        public Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId)
        {
            eventService.GetEventAsync(eventId).Wait();
            var bk = new BookingModel(eventId, null);
            Bookings.Add(bk);
            var cbkdto = new CreatedBookingDto { Id = bk.Id, EventId = bk.EventId, Status = bk.Status};
            return Task.FromResult(cbkdto);
        }

        public Task<BookingModel?> GetBookingByIdAsync(Guid bookingId)
        {
            var bk = Bookings.FirstOrDefault(e => e.Id.Equals(bookingId));
            if (bk == null)
                throw new NotFoundException($"Booking with id {bookingId} not found");
            return Task.FromResult(bk);
        }
    }
}
