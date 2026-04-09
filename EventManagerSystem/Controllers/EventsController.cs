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
        public IActionResult GetAllEvents()
        {
            return Ok(_eventService.GetAllEvents());
        }

        [HttpGet("{id}")]
        public IActionResult GetEventById(Guid id)
        {
            var _event = _eventService.GetEvent(id);
            if (_event is not null)
                return Ok(_event);
            return NotFound();
        }

        [HttpPost]
        public IActionResult Post([FromBody] EventModel model)
        {
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] EventModel model) 
        { 
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var isDeletedSuccessfully = _eventService.DeleteEvent(id);
            if (isDeletedSuccessfully)
                return NoContent();
            return NotFound();
        }
    }
}
