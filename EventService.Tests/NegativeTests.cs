using EventManagerSystem.DataAccess;
using EventManagerSystem.DTO.Events;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Repositories.Event;
using EventManagerSystem.Services;
using EventManagerSystem.Services.EventService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace EventService.Tests
{
    public class NegativeTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;

        public NegativeTests()
        {
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventService, EventManagerSystem.Services.EventService.EventService>();

            _serviceProvider = services.BuildServiceProvider();

            _scope = _serviceProvider.CreateScope();

            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        }

        [Fact]
        public async Task GetEventByIdAsync_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _eventService.GetEventAsync(nonExistingId));
        }

        [Fact]
        public async Task UpdateEventAsync_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            var dto = new UpdateEventDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _eventService.UpdateEventAsync(nonExistingId, dto));
        }

        [Fact]
        public async Task CreateEventAsync_EndAtBeforeStartAt_ThrowsValidationException()
        {
            // Arrange
            var dto = new CreateEventDto
            {
                Title = "Test Event",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(5),
                EndAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _eventService.CreateEventAsync(dto));
        }

        [Fact]
        public async Task CreateEventAsync_EmptyTitle_ThrowsValidationException()
        {
            // Arrange
            var dto = new CreateEventDto
            {
                Title = "",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _eventService.CreateEventAsync(dto));
        }

        [Fact]
        public async Task UpdateEventAsync_EndAtBeforeStartAt_ThrowsValidationException()
        {
            // Arrange
            var createEventDto = new CreateEventDto
            {
                Title = "Test Event",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                TotalSeats = 1
            };

            var createdEvent = await _eventService.CreateEventAsync(createEventDto);

            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                StartAt = DateTime.UtcNow.AddDays(5),
                EndAt = DateTime.UtcNow.AddDays(1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _eventService.UpdateEventAsync(createdEvent.Id, updateEventDto));
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }
    }
}
