using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Enums;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;

namespace EventManagerSystem.Services.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly List<BookingModel> _bookings = new();
        private IEventService eventService;
        private readonly SemaphoreSlim _bookingLock = new SemaphoreSlim(1, 1);

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
                _bookings.Add(bk);
                var cbkdto = new CreatedBookingDto { Id = bk.Id, EventId = bk.EventId, Status = bk.Status };
                return cbkdto;
            }
            finally
            {
                _bookingLock.Release();
            }
        }

        public async Task<GetBookingDto?> GetBookingByIdAsync(Guid bookingId)
        {
            await _bookingLock.WaitAsync(new CancellationToken());

            try
            {
                var booking = _bookings.FirstOrDefault(e => e.Id == bookingId);

                if (booking is null)
                {
                    throw new NotFoundException($"Booking with id {bookingId} not found");
                }

                return new GetBookingDto
                {
                    Id = booking.Id,
                    Status = booking.Status,
                    ProcessedAt = booking.ProcessedAt
                };
            }
            finally
            {
                _bookingLock.Release();
            }
        }

        public async Task<IEnumerable<BookingModel>> GetPendingBookingsAsync()
        {
            await _bookingLock.WaitAsync(new CancellationToken());

            try
            {
                return _bookings
                    .Where(b => b.Status == BookingStatus.Pending)
                    .ToList();
            }
            finally
            {
                _bookingLock.Release();
            }
        }

        public async Task UpdateBookingAsync(BookingModel bookingForUpdating)
        {
            await _bookingLock.WaitAsync(new CancellationToken());

            try
            {

                var booking = _bookings.FirstOrDefault(x => x.Id == bookingForUpdating.Id);     
                if (booking is null)
                {
                    throw new NotFoundException($"Booking with id {bookingForUpdating.Id} not found");
                }
                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = DateTime.UtcNow;
            }
            finally
            {
                _bookingLock.Release();
            }
        }

        public async Task RejectBookingAsync(BookingModel bookingForRejecting)
        {
            EventModel _event = null;
            try
            {
                _event = await eventService.GetEventAsync(bookingForRejecting.EventId);          
            }
            catch (NotFoundException ex)
            {

            }
            finally
            {
                if (_event != null)
                {
                    eventService.ReleaseSeats(_event.Id);
                }
                bookingForRejecting.Status = BookingStatus.Rejected;
                bookingForRejecting.ProcessedAt = DateTime.UtcNow;
            }
        }


    }
}
