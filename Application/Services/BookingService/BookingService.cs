using Application.DTO.Events;
using Application.DTO.Bookings;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Application.Repositories.Booking;

namespace Application.Services.BookingService
{
    public class BookingService : IBookingService
    {

        private readonly IEventService eventService;
        private readonly IBookingRepository _bookingRepository;
        private static readonly SemaphoreSlim _bookingLock = new SemaphoreSlim(1, 1);

        public BookingService(IEventService eventService, IBookingRepository bookingRepository)
        {
            this.eventService = eventService;
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingModel> GetBookingModelByIdAsync(Guid id)
        {
            await _bookingLock.WaitAsync(new CancellationToken());
            try
            {
                var booking = await _bookingRepository.GetBookingByIdAsync(id);
                if (booking is null)
                {
                    throw new NotFoundException($"Booking with id {id} not found");
                }
                return booking;
            }
            finally
            {
                _bookingLock.Release();
            }
        }
        public async Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId)
        {   
            await _bookingLock.WaitAsync(new CancellationToken());
            try
            {
                await eventService.GetEventAsync(eventId);
                await eventService.TryReserveSeats(eventId);
                var booking = new BookingModel(eventId);
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
                   EventId = bk.EventId,
                   Status = bk.Status,
                   CreatedAt = bk.CreatedAt,
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

        public async Task UpdateBookingAsync(Guid bookingId)
        {
            await _bookingLock.WaitAsync(new CancellationToken());

            try
            {
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);  
                if (booking is null)
                {
                    throw new NotFoundException($"Booking with id {bookingId} not found");
                }
                booking.UpdateStatus(BookingStatus.Confirmed);
                
                _bookingRepository.UpdateBooking(booking);
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
            if (booking is null)
            {
                throw new NotFoundException($"Booking with id {bookingForRejecting} not found");
            }
            EventDto? ev = null;
            try
            {
                 ev = await eventService.GetEventAsync(booking.EventId);          
            }
            finally
            {
                if (ev != null)
                {
                    await eventService.ReleaseSeats(ev.Id);
                }

                booking.UpdateStatus(BookingStatus.Rejected);
                _bookingRepository.UpdateBooking(booking);
                await _bookingRepository.SaveChangesAsync();
            }
        }
    }
}
