using EventManagerSystem.DataAccess;
using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Enums;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using EventManagerSystem.Repositories.Booking;
using Microsoft.EntityFrameworkCore;

namespace EventManagerSystem.Services.BookingService
{
    internal class BookingService : IBookingService
    {

        private IEventService eventService;
        private IBookingRepository _bookingRepository;
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
                var bk = new BookingModel(eventId, null);
                _context.Bookings.Add(bk);
                await _context.SaveChangesAsync();
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
               return await _bookingRepository.GetBookingByIdAsync(bookingId);
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
                return _context.Bookings
                    .Where(b => b.Status == BookingStatus.Pending)
                    .ToList();
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

                var booking = await _context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingForUpdating);     
                if (booking is null)
                {
                    throw new NotFoundException($"Booking with id {bookingForUpdating} not found");
                }
                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            finally
            {
                _bookingLock.Release();
            }
        }

        public async Task RejectBookingAsync(Guid bookingForRejecting)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingForRejecting);
            EventModel _event = null;
            try
            {
                _event = await eventService.GetEventAsync(booking.EventId);          
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
                
                booking.Status = BookingStatus.Rejected;
                booking.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
