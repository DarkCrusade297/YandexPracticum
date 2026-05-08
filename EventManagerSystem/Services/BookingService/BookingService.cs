using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Enums;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using Microsoft.Extensions.Logging;

namespace EventManagerSystem.Services.BookingService
{
    public class BookingService : IBookingService
    {
        public List<BookingModel> Bookings { get; set; } = new List<BookingModel>();
        private IEventService eventService;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IEventService eventService, ILogger<BookingService> logger)
        {
            this.eventService = eventService;
            this._logger = logger;
        }

        public Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId)
        {
            eventService.GetEventAsync(eventId).Wait();
            var bk = new BookingModel(eventId, null);
            Bookings.Add(bk);
            var cbkdto = new CreatedBookingDto { Id = bk.Id, EventId = bk.EventId, Status = bk.Status};
            return Task.FromResult(cbkdto);
        }

        public Task<GetBookingDto?> GetBookingByIdAsync(Guid bookingId)
        {
            var bk = Bookings.FirstOrDefault(e => e.Id.Equals(bookingId));
            if (bk == null)
                throw new NotFoundException($"Booking with id {bookingId} not found");
            var getBooking = new GetBookingDto { Id =  bookingId, Status = bk.Status, ProcessedAt = bk.ProcessedAt };
            return Task.FromResult(getBooking);
        }

        public async Task<IEnumerable<BookingModel>> GetPendingBookingsAsync()
        {
            return Bookings.Where(b => b.Status == BookingStatus.Pending);
        }

        public Task ConfirmBookingAsync(Guid bookingId)
        {
            var booking = Bookings.First(x => x.Id == bookingId);
            booking.Status = BookingStatus.Confirmed;
            booking.ProcessedAt = DateTime.UtcNow;
            return Task.CompletedTask;
        }
    }
}
