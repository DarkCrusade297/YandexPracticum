using System.ComponentModel.DataAnnotations;
using Event.Application.DTO;
using Event.Domain.Exceptions;

namespace EventService.Tests;

public sealed class NegativeTests
{
    [Fact]
    public async Task GetEventAsync_NonExistingId_ThrowsNotFoundException()
    {
        var context = new EventServiceTestContext();
        await Assert.ThrowsAsync<NotFoundException>(() => context.Service.GetEventAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateEventAsync_NonExistingId_ThrowsNotFoundException()
    {
        var context = new EventServiceTestContext();
        await Assert.ThrowsAsync<NotFoundException>(() => context.Service.UpdateEventAsync(Guid.NewGuid(), ValidUpdateDto()));
    }

    [Fact]
    public async Task CreateEventAsync_EndBeforeStart_ThrowsValidationException()
    {
        var context = new EventServiceTestContext();
        await Assert.ThrowsAsync<ValidationException>(() => context.Service.CreateEventAsync(new CreateEventDto
        {
            Title = "Test event", StartAt = DateTime.UtcNow.AddDays(2), EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = 1
        }));
    }

    [Fact]
    public async Task CreateEventAsync_EmptyTitle_ThrowsValidationException()
    {
        var context = new EventServiceTestContext();
        await Assert.ThrowsAsync<ValidationException>(() => context.Service.CreateEventAsync(new CreateEventDto
        {
            Title = string.Empty, StartAt = DateTime.UtcNow.AddDays(1), EndAt = DateTime.UtcNow.AddDays(2), TotalSeats = 1
        }));
    }

    [Fact]
    public async Task UpdateEventAsync_EndBeforeStart_ThrowsValidationException()
    {
        var model = PositiveTests.CreateModel("Test event");
        var context = new EventServiceTestContext(model);
        await Assert.ThrowsAsync<ValidationException>(() => context.Service.UpdateEventAsync(model.Id, new UpdateEventDto
        {
            Title = "Updated", StartAt = DateTime.UtcNow.AddDays(2), EndAt = DateTime.UtcNow.AddDays(1)
        }));
    }

    private static UpdateEventDto ValidUpdateDto() => new()
    {
        Title = "Updated", StartAt = DateTime.UtcNow.AddDays(1), EndAt = DateTime.UtcNow.AddDays(2)
    };
}
