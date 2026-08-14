using Application.Common.Interfaces;
using Infrastructure.DataAccess;
using Application.DTO.Bookings;
using Application.DTO.Events;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Repositories.Booking;
using Infrastructure.Repositories.Event;
using Application.Services;
using Application.Services.BookingService;
using Application.Services.EventService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application.Repositories.Booking;

namespace BookingServices.Tests
{
    public class PositiveTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public PositiveTests()
        {
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();

            _serviceProvider = services.BuildServiceProvider();

            _scope = _serviceProvider.CreateScope();

            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_ReturnsBookingInfo()
        {
            // Arrange
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(booking);
            Assert.Equal(createdEvent.Id, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_AvailableSeatsMinus()
        {
            // Arrange
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var eventDto = await _eventService.GetEventAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(eventDto);
            Assert.NotNull(booking);
            Assert.Equal(createdEvent.Id, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Equal(9, eventDto.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_BookingsAfterLimit()
        {
            // Arrange
            var createEventDto = CreateDefaultEventDto(totalSeats: 1);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(booking);
            Assert.Equal(createdEvent.Id, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);

            await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>
                _bookingService.CreateBookingAsync(createdEvent.Id));
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_BookingsBeforeLimit()
        {
            // Arrange
            var createEventDto = CreateDefaultEventDto(totalSeats: 3);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var booking1 = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var booking2 = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var booking3 = await _bookingService.CreateBookingAsync(createdEvent.Id);

            var eventDto = await _eventService.GetEventAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(eventDto);
            Assert.NotNull(booking1);
            Assert.NotNull(booking2);
            Assert.NotNull(booking3);

            Assert.Equal(createdEvent.Id, booking1.EventId);
            Assert.Equal(createdEvent.Id, booking2.EventId);
            Assert.Equal(createdEvent.Id, booking3.EventId);

            Assert.Equal(BookingStatus.Pending, booking1.Status);
            Assert.Equal(BookingStatus.Pending, booking2.Status);
            Assert.Equal(BookingStatus.Pending, booking3.Status);

            Assert.Equal(0, eventDto.AvailableSeats);

            Assert.NotEqual(booking1.Id, booking2.Id);
            Assert.NotEqual(booking1.Id, booking3.Id);
            Assert.NotEqual(booking2.Id, booking3.Id);
        }

        [Fact]
        public async Task CreateBookingAsync_CreateFewBookingForOneEvent_ReturnsBookingInfo()
        {
            // Arrange
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var booking1 = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var booking2 = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(booking1);
            Assert.NotNull(booking2);

            Assert.Equal(createdEvent.Id, booking1.EventId);
            Assert.Equal(createdEvent.Id, booking2.EventId);

            Assert.Equal(BookingStatus.Pending, booking1.Status);
            Assert.Equal(BookingStatus.Pending, booking2.Status);

            Assert.NotEqual(booking1.Id, booking2.Id);
        }

        [Fact]
        public async Task GetBookingAsync_ValidDto_ReturnsBooking()
        {
            // Arrange
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var booking = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Act
            var createdBooking = await _bookingService.GetBookingByIdAsync(booking.Id);

            // Assert
            Assert.NotNull(createdBooking);
            Assert.Equal(booking.Id, createdBooking.Id);
            Assert.Equal(BookingStatus.Pending, createdBooking.Status);
            Assert.Null(createdBooking.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingAsync_ValidDto_ReturnsConfirmedStatus()
        {
            // Arrange
            var createEventDto = CreateDefaultEventDto(totalSeats: 10);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var bookingCreatedDto = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Act
            await _bookingService.UpdateBookingAsync(bookingCreatedDto.Id);

            var createdBooking = await _bookingService.GetBookingByIdAsync(bookingCreatedDto.Id);

            // Assert
            Assert.NotNull(createdBooking);
            Assert.NotNull(bookingCreatedDto);
            Assert.Equal(bookingCreatedDto.Id, createdBooking.Id);
            Assert.Equal(BookingStatus.Confirmed, createdBooking.Status);
            Assert.NotNull(createdBooking.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingAsync_ValidDto_ReturnsRejectedStatus()
        {
            // Arrange
            var createEventDto = CreateDefaultEventDto(totalSeats: 1);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var bookingCreatedDto = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var bookingDto = await _bookingService.GetBookingByIdAsync(bookingCreatedDto.Id);

            // Act
            await _bookingService.RejectBookingAsync(bookingDto.Id);

            var rejectedBooking = await _bookingService.GetBookingByIdAsync(bookingDto.Id);

            var secondBooking = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(rejectedBooking);
            Assert.NotNull(bookingCreatedDto);
            Assert.NotNull(bookingDto);
            Assert.Equal(bookingDto.Id, rejectedBooking.Id);
            Assert.Equal(BookingStatus.Rejected, rejectedBooking.Status);
            Assert.NotNull(rejectedBooking.ProcessedAt);

            Assert.NotNull(secondBooking);
            Assert.Equal(createdEvent.Id, secondBooking.EventId);
            Assert.Equal(BookingStatus.Pending, secondBooking.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_TenConcurrentRequests_ReturnsTenUniqueBookings()
        {
            // Arrange
            const int concurrentRequests = 10;

            var createEventDto = CreateDefaultEventDto(totalSeats: 10);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var tasks = Enumerable.Range(0, concurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();

                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    return await bookingService.CreateBookingAsync(createdEvent.Id);
                }))
                .ToArray();

            var bookings = await Task.WhenAll(tasks);

            using var checkScope = _serviceProvider.CreateScope();

            var eventService = checkScope.ServiceProvider.GetRequiredService<IEventService>();

            var updatedEvent = await eventService.GetEventAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(updatedEvent);
            Assert.Equal(10, bookings.Length);

            Assert.All(bookings, Assert.NotNull);
            Assert.All(bookings, b => Assert.Equal(createdEvent.Id, b!.EventId));
            Assert.All(bookings, b => Assert.Equal(BookingStatus.Pending, b!.Status));

            var uniqueIds = bookings
                .Select(b => b!.Id)
                .Distinct()
                .ToList();

            Assert.Equal(10, uniqueIds.Count);
            Assert.Equal(0, updatedEvent.AvailableSeats);
        }


        [Fact]
        public async Task CreateBookingAsync_TwentyConcurrentRequestsFiveSeats_ReturnsExactlyFiveSuccessAndFifteenExceptions()
        {
            // Arrange
            const int concurrentRequests = 20;

            var createEventDto = CreateDefaultEventDto(totalSeats: 5);

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var tasks = Enumerable.Range(0, concurrentRequests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();

                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    return await SafeCreateBookingAsync(bookingService, createdEvent.Id);
                }))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            var succeeded = results
                .Where(r => r.Success)
                .ToList();

            var failed = results
                .Where(r => !r.Success)
                .ToList();

            using var checkScope = _serviceProvider.CreateScope();

            var context = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var updatedEvent = await context.Events
                .AsNoTracking()
                .FirstAsync(e => e.Id == createdEvent.Id);

            // Assert
            Assert.Equal(5, succeeded.Count);
            Assert.Equal(15, failed.Count);

            Assert.All(succeeded, r => Assert.NotNull(r.Booking));
            Assert.All(succeeded, r => Assert.Equal(createdEvent.Id, r.Booking!.EventId));
            Assert.All(succeeded, r => Assert.Equal(BookingStatus.Pending, r.Booking!.Status));

            Assert.All(failed, r => Assert.IsType<NoAvailableSeatsException>(r.Error));

            var uniqueIds = succeeded
                .Select(r => r.Booking!.Id)
                .Distinct()
                .ToList();

            Assert.Equal(5, uniqueIds.Count);

            Assert.Equal(0, updatedEvent.AvailableSeats);
        }


        private static CreateEventDto CreateDefaultEventDto(int totalSeats)
        {
            return new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = totalSeats
            };
        }

        private static async Task<(bool Success, CreatedBookingDto? Booking, Exception? Error)> SafeCreateBookingAsync(
            IBookingService bookingService,
            Guid eventId)
        {
            try
            {
                var booking = await bookingService.CreateBookingAsync(eventId);

                return (true, booking, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex);
            }
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }
    }
}
