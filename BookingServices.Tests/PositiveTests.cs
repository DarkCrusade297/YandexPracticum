using EventManagerSystem.DTO.Events;
using EventManagerSystem.Enums;
using EventManagerSystem.Models;
using EventManagerSystem.Services;
using EventManagerSystem.Services.BookingService;
using EventManagerSystem.Services.EventService;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

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
                EndAt = DateTime.UtcNow.AddDays(31)
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
        public async Task CreateBookingAsync_CreateFewBookingForOneEvent_ReturnsBookingInfo()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31)
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
                EndAt = DateTime.UtcNow.AddDays(31)
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
                EndAt = DateTime.UtcNow.AddDays(31)
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var _bookingCreatedDto = await _bookingService.CreateBookingAsync(createdEvent.Id);
            var _bookingDto = await _bookingService.GetBookingByIdAsync(_bookingCreatedDto.Id);
            var _booking = new BookingModel(_bookingCreatedDto.Id, createdEvent.Id, _bookingCreatedDto.Status, _bookingDto.ProcessedAt);
            await _bookingService.UpdateBookingAsync(_booking);

            //Act
            var createdBooking = await _bookingService.GetBookingByIdAsync(_booking.Id);

            // Assert
            Assert.NotNull(createdBooking);
            Assert.Equal(_booking.Id, createdBooking.Id);
            Assert.Equal(BookingStatus.Confirmed, createdBooking.Status);
            Assert.NotNull(createdBooking.ProcessedAt);
        }
    }
}