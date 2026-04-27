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
        public async Task<ActionResult<List<EventModel>>> GetAllEvents([FromQuery] string? title, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {

            var events = await _eventService.GetAllEventsAsync(title, from, to);
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventModel>> GetEventById(Guid id)
        {
            var ev = await _eventService.GetEventAsync(id);
            return Ok(ev);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateEventDto dto)
        {
            var ev = await _eventService.CreateEventAsync(dto);
            return CreatedAtAction(nameof(GetEventById), new { id = ev.Id }, ev);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventDto eventDto)
        {
            await _eventService.UpdateEventAsync(id, eventDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }
    }
}
