using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.DTO.Events;
using EventManagerSystem.Enums;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using EventManagerSystem.Services.BookingService;
using EventManagerSystem.Services.EventService;

namespace BookingServices.Tests
{
    public class PositiveTests
    {
        private readonly EventService _eventService;
        private readonly BookingService _bookingService;

        //  онструктор выполн€етс€ перед каждым тестом
        public PositiveTests()
        {
            _eventService = new EventService();
            _bookingService = new BookingService(_eventService);
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_ReturnsBookingInfo()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 10,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var _booking = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(_booking);
            Assert.Equal(createdEvent.Id, _booking.EventId);
            Assert.Equal(BookingStatus.Pending, _booking.Status);        
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_AvailableSeatsMinus()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 10,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var _booking = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var _event = await _eventService.GetEventAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(_booking);
            Assert.Equal(createdEvent.Id, _booking.EventId);
            Assert.Equal(BookingStatus.Pending, _booking.Status);
            Assert.Equal(_event.AvailableSeats, 9);
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_BookingsAfterLimit()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 1,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var _booking1 = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(_booking1);
            Assert.Equal(createdEvent.Id, _booking1.EventId);
            Assert.Equal(BookingStatus.Pending, _booking1.Status);
            await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(createdEvent.Id));
        }

        [Fact]
        public async Task CreateBookingAsync_ValidDto_BookingsBeforeLimit()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 3,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var _booking1 = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var _booking2 = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var _booking3 = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var _event = await _eventService.GetEventAsync(createdEvent.Id);
            // Assert
            Assert.NotNull(_booking1);
            Assert.NotNull(_booking2);
            Assert.NotNull(_booking2);
            Assert.Equal(createdEvent.Id, _booking1.EventId);
            Assert.Equal(createdEvent.Id, _booking2.EventId);
            Assert.Equal(createdEvent.Id, _booking3.EventId);
            Assert.Equal(BookingStatus.Pending, _booking1.Status);
            Assert.Equal(BookingStatus.Pending, _booking2.Status);
            Assert.Equal(BookingStatus.Pending, _booking3.Status);
            Assert.Equal(_event.AvailableSeats, 0);
            Assert.NotEqual(_booking1.Id, _booking2.Id);
            Assert.NotEqual(_booking1.Id, _booking3.Id);
            Assert.NotEqual(_booking2.Id, _booking3.Id);
        }

        [Fact]
        public async Task CreateBookingAsync_CreateFewBookingForOneEvent_ReturnsBookingInfo()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 10,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var _booking1 = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var _booking2 = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(_booking1);
            Assert.NotNull(_booking2);
            Assert.Equal(createdEvent.Id, _booking1.EventId);
            Assert.Equal(createdEvent.Id, _booking2.EventId);
            Assert.Equal(BookingStatus.Pending, _booking1.Status);
            Assert.Equal(BookingStatus.Pending, _booking2.Status);
            Assert.NotEqual(_booking1.Id, _booking2.Id);
        }

        [Fact]
        public async Task GetBookingAsync_ValidDto_ReturnsBooking()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 10,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var _booking = await _bookingService.CreateBookingAsync(createdEvent.Id);

            //Act
            var createdBooking = await _bookingService.GetBookingByIdAsync(_booking.Id);

            // Assert
            Assert.NotNull(createdBooking);
            Assert.Equal(_booking.Id, createdBooking.Id);
            Assert.Equal(BookingStatus.Pending, createdBooking.Status);
            Assert.Null(createdBooking.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingAsync_ValidDto_ReturnsConfirmedStatus()
        {

            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 10,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var _bookingCreatedDto = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var _bookingDto = await _bookingService.GetBookingByIdAsync(_bookingCreatedDto.Id);
            var _booking = new BookingModel(_bookingCreatedDto.Id, createdEvent.Id, _bookingCreatedDto.Status, _bookingDto.ProcessedAt);
            await _bookingService.UpdateBookingAsync(_booking.Id);

            //Act
            var createdBooking = await _bookingService.GetBookingByIdAsync(_booking.Id);

            // Assert
            Assert.NotNull(createdBooking);
            Assert.Equal(_booking.Id, createdBooking.Id);
            Assert.Equal(BookingStatus.Confirmed, createdBooking.Status);
            Assert.NotNull(createdBooking.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingAsync_ValidDto_ReturnsRejectedStatus()
        {

            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 1,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var _bookingCreatedDto = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var _bookingDto = await _bookingService.GetBookingByIdAsync(_bookingCreatedDto.Id);

            //Act
            await _bookingService.RejectBookingAsync(_bookingDto.Id);
            var createdBooking = await _bookingService.GetBookingByIdAsync(_bookingDto.Id);
            var _bookingCreatedDto2 = await _bookingService.CreateBookingAsync(createdEvent.Id);

            // Assert
            Assert.NotNull(createdBooking);
            Assert.Equal(_bookingDto.Id, createdBooking.Id);
            Assert.Equal(BookingStatus.Rejected, createdBooking.Status);
            Assert.NotNull(createdBooking.ProcessedAt);
            Assert.NotNull(_bookingCreatedDto2);
        }

        [Fact]
        public async Task CreateBookingAsync_TenConcurrentRequests_ReturnsTenUniqueBookings()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 10,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => _bookingService.CreateBookingAsync(createdEvent.Id))
                .ToArray();

            var bookings = await Task.WhenAll(tasks);

            var updatedEvent = await _eventService.GetEventAsync(createdEvent.Id);

            // Assert
            Assert.Equal(10, bookings.Length);
            Assert.All(bookings, b => Assert.NotNull(b));
            Assert.All(bookings, b => Assert.Equal(createdEvent.Id, b.EventId));
            Assert.All(bookings, b => Assert.Equal(BookingStatus.Pending, b.Status));

            var uniqueIds = bookings.Select(b => b.Id).Distinct().ToList();
            Assert.Equal(10, uniqueIds.Count);

            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_TwentyConcurrentRequestsFiveSeats_ReturnsExactlyFiveSuccessAndFifteenExceptions()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 5,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            // Act
            var tasks = Enumerable.Range(0, 20)
                .Select(_ => SafeCreateBookingAsync(createdEvent.Id))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            var succeeded = results.Where(r => r.Success).ToList();
            var failed = results.Where(r => !r.Success).ToList();

            var updatedEvent = await _eventService.GetEventAsync(createdEvent.Id);

            // Assert
            Assert.Equal(5, succeeded.Count);
            Assert.Equal(15, failed.Count);

            Assert.All(succeeded, r => Assert.NotNull(r.Booking));
            Assert.All(succeeded, r => Assert.Equal(createdEvent.Id, r.Booking!.EventId));
            Assert.All(succeeded, r => Assert.Equal(BookingStatus.Pending, r.Booking!.Status));

            Assert.All(failed, r => Assert.IsType<NoAvailableSeatsException>(r.Error));

            var uniqueIds = succeeded.Select(r => r.Booking!.Id).Distinct().ToList();
            Assert.Equal(5, uniqueIds.Count);

            Assert.Equal(0, updatedEvent.AvailableSeats);
        }

        private async Task<(bool Success, CreatedBookingDto? Booking, Exception? Error)> SafeCreateBookingAsync(Guid eventId)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(eventId);
                return (true, booking, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex);
            }
        }
    }
}