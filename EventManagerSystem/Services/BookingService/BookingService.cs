using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Enums;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using EventManagerSystem.Repositories.Booking;

namespace EventManagerSystem.Services.BookingService
{
    internal class BookingService : IBookingService
    {

        private readonly IEventService eventService;
        private readonly IBookingRepository _bookingRepository;
        private static readonly SemaphoreSlim _bookingLock = new SemaphoreSlim(1, 1);

        public BookingService(IEventService eventService, IBookingRepository bookingRepository)
        {
            this.eventService = eventService;
            _bookingRepository = bookingRepository;
        }

        public async Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId)
        {   
            await _bookingLock.WaitAsync(new CancellationToken());
            try
            {
                await eventService.GetEventAsync(eventId);
                var tryReserve = await eventService.TryReserveSeats(eventId);
                if (!tryReserve)
                    throw new NoAvailableSeatsException("No available seats for this event");
                var booking = new BookingModel(eventId, null);
                var bk = await _bookingRepository.CreateBookingAsync(booking);
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
               var bk = await _bookingRepository.GetBookingByIdAsync(bookingId);
               if (bk is null)
               {
                   throw new NotFoundException($"Booking with id {bookingId} not found");
               }
               return new GetBookingDto
               {
                   Id = bk.Id,
                   Status = bk.Status,
                   ProcessedAt = bk.ProcessedAt
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
               return await _bookingRepository.GetPendingBookingsAsync();
            }
            finally
            {
                _bookingLock.Release();
            }
        }

        public async Task UpdateBookingAsync(Guid bookingForUpdating)
        {
            await _bookingLock.WaitAsync(new CancellationToken());

            try
            {
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingForUpdating);  
                if (booking is null)
                {
                    throw new NotFoundException($"Booking with id {bookingForUpdating} not found");
                }
                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = DateTime.UtcNow;
                await _bookingRepository.SaveChangesAsync();
            }
            finally
            {
                _bookingLock.Release();
            }
        }

        public async Task RejectBookingAsync(Guid bookingForRejecting)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingForRejecting);
            EventModel? _event = null;
            try
            {
                _event = await eventService.GetEventAsync(booking.EventId);          
            }
            finally
            {
                if (_event != null)
                {
                    await eventService.ReleaseSeats(_event.Id);
                }
                
                booking.Status = BookingStatus.Rejected;
                booking.ProcessedAt = DateTime.UtcNow;
                await _bookingRepository.SaveChangesAsync();
            }
        }
    }
}
