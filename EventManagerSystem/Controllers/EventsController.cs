using EventManagerSystem.DTO;
using EventManagerSystem.Models;
using EventManagerSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerSystem.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(ILogger<EventsController> logger, IEventService eventService)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto>> GetAllEvents([FromQuery] string? title,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? page,
            [FromQuery] int? pageSize)
        {

            var events = await _eventService.GetAllEventsAsync(title, from, to, page, pageSize);
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventModel>> GetEventById(Guid id)
        {
            var ev = await _eventService.GetEventAsync(id);
            return Ok(ev);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var ev = await _eventService.CreateEventAsync(dto);
            return CreatedAtAction(nameof(GetEventById), new { id = ev.Id }, ev);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEventById(Guid id, [FromBody] UpdateEventDto eventDto)
        {
            await _eventService.UpdateEventAsync(id, eventDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventById(Guid id)
        {
            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }
    }
}
