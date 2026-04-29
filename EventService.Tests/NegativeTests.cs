using EventManagerSystem.DTO;
using EventManagerSystem.Exceptions;
using EventManagerSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventService.Tests
{
    public class NegativeTests
    {
        private readonly EventManagerSystem.Services.EventService _sut = new EventManagerSystem.Services.EventService();
        [Fact]
        public async Task GetEventByIdAsync_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetEventAsync(nonExistingId));
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
            await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateEventAsync(nonExistingId, dto));
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
                EndAt = DateTime.UtcNow.AddDays(1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateEventAsync(dto));
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
                EndAt = DateTime.UtcNow.AddDays(2)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateEventAsync(dto));
        }

        [Fact]
        public async Task UpdateEventAsync_EndAtBeforeStartAt_ThrowsValidationException()
        {
            // Arrange
            var _event = new CreateEventDto
            {
                Title = "Test Event",
                Description = "Description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };
            var createdEvent =  await _sut.CreateEventAsync(_event);

            var updateEventDto = new UpdateEventDto
            {
                Title = "Updated Title",
                StartAt = DateTime.UtcNow.AddDays(5),
                EndAt = DateTime.UtcNow.AddDays(1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateEventAsync(createdEvent.Id, updateEventDto));
        }
    }
}
