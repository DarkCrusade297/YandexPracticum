using Application.Common.Interfaces;
using Infrastructure.DataAccess;
using Application.DTO.Events;
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
    public class NegativeTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;

        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        private readonly AppDbContext _context;

        public NegativeTests()
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
            _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }

        [Fact]
        public async Task CreateBookingAsync_NotValidEventId_ReturnsNotFoundException()
        {
            // Arrange
            var nonExistingEventId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _bookingService.CreateBookingAsync(nonExistingEventId));

            Assert.Empty(await _context.Bookings.ToListAsync());
        }

        [Fact]
        public async Task GetBookingAsync_NotValidBookingId_ReturnsNotFoundException()
        {
            // Arrange
            var nonExistingBookingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _bookingService.GetBookingByIdAsync(nonExistingBookingId));

            Assert.Empty(await _context.Bookings.ToListAsync());
        }

        [Fact]
        public async Task CreateBookingAsync_CreateBookingAfterEventWasDeleted_ReturnsNotFoundException()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Title",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(30),
                EndAt = DateTime.UtcNow.AddDays(31),
                TotalSeats = 10
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);
            var eventId = createdEvent.Id;

            Assert.NotNull(createdEvent);
            Assert.Equal("Title", createdEvent.Title);

            // Act
            await _eventService.DeleteEventAsync(eventId);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _eventService.GetEventAsync(eventId));

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _bookingService.CreateBookingAsync(eventId));

            Assert.Empty(await _context.Bookings.ToListAsync());
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }
    }
}
