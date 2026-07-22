using EventManagerSystem.DTO.Events;
using EventManagerSystem.Enums;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using EventManagerSystem.Services;
using EventManagerSystem.Services.BookingService;
using EventManagerSystem.Services.EventService;
using Moq;

namespace BookingServices.Tests
{
    public class NegativeTests
    {
        private readonly EventService _eventService;
        private readonly BookingService _bookingService;

        public NegativeTests()
        {
            _eventService = new EventService();
            _bookingService = new BookingService(_eventService);
        }

        [Fact]
        public async Task CreateBookingAsync_NotValidEventId_ReturnsNotFoundException()
        {
            // Arrange
            var nonExistingEventId = Guid.NewGuid();

            //Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _bookingService.CreateBookingAsync(nonExistingEventId));
            Assert.Empty(_bookingService._bookings);
        }

        [Fact]
        public async Task GetBookingAsync_NotValidBookingId_ReturnsNotFoundException()
        {
            // Arrange
            var nonExistingBookingId = Guid.NewGuid();

            //Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _bookingService.GetBookingByIdAsync(nonExistingBookingId));
            Assert.Empty(_bookingService._bookings);
        }

        [Fact]
        public async Task CreateBookingAsync_CreateBookingAfterEventWasDeleted_ReturnsNotFoundException()
        {
            // Arrange - создаём событие
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 10,
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var eventId = createdEvent.Id;

            Assert.NotNull(createdEvent);
            Assert.Equal("Title", createdEvent.Title);

            // Act & Assert
            await _eventService.DeleteEventAsync(eventId);
            await Assert.ThrowsAsync<NotFoundException>(() => _eventService.GetEventAsync(eventId));
            await Assert.ThrowsAsync<NotFoundException>(() => _bookingService.CreateBookingAsync(eventId));
        }
    }
}