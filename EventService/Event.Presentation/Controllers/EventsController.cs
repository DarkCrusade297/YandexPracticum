using Event.Application.DTO;
using Event.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event.Presentation.Controllers;

[ApiController]
[Route("events")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet, Authorize]
    public async Task<ActionResult<PaginatedResultDto>> GetAllEvents(string? title, DateTime? from, DateTime? to, int? page, int? pageSize) =>
        Ok(await eventService.GetAllEventsAsync(title, from, to, page, pageSize));

    [HttpGet("{id:guid}"), Authorize]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id) => Ok(await eventService.GetEventAsync(id));

    [HttpGet("/internal/events/{id:guid}"), ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult<EventDto>> GetInternalEventById(Guid id) => Ok(await eventService.GetEventAsync(id));

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEvent(CreateEventDto dto)
    {
        var ev = await eventService.CreateEventAsync(dto);
        return CreatedAtAction(nameof(GetEventById), new { id = ev.Id }, ev);
    }

    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEvent(Guid id, UpdateEventDto dto)
    {
        await eventService.UpdateEventAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        await eventService.DeleteEventAsync(id);
        return NoContent();
    }

    [HttpPost("/internal/events/{id:guid}/reserve"), ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> ReserveSeat(Guid id)
    {
        await eventService.ReserveSeatsAsync(id);
        return NoContent();
    }

    [HttpPost("/internal/events/{id:guid}/release"), ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> ReleaseSeat(Guid id)
    {
        await eventService.ReleaseSeatsAsync(id);
        return NoContent();
    }
}
