using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Enums;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;

namespace EventManagerSystem.Services.BookingService
{
    public class BookingService : IBookingService
    {
        public List<BookingModel> Bookings { get; set; } = new List<BookingModel>();
        private IEventService eventService;
        private readonly SemaphoreSlim _bookingLock = new SemaphoreSlim(1,1);

        public BookingService(IEventService eventService)
        {
            this.eventService = eventService;
        }

        public async Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId)
        {
            await eventService.GetEventAsync(eventId);
            await _bookingLock.WaitAsync(new CancellationToken());
            try
            {
                var tryReserve = eventService.TryReserveSeats(eventId);
                if (!tryReserve)
                    throw new NoAvailableSeatsException("No available seats for this event");
                var bk = new BookingModel(eventId, null);
                Bookings.Add(bk);
                var cbkdto = new CreatedBookingDto { Id = bk.Id, EventId = bk.EventId, Status = bk.Status };
                return cbkdto;
            }
            finally
            {
                _bookingLock.Release();
            }
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
