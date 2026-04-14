using EventManagerSystem.DTO;
using EventManagerSystem.Models;
using EventManagerSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Linq.Expressions;

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
        public ActionResult<List<EventModel>> GetAllEvents()
        {
            return Ok(_eventService.GetAllEvents());
        }

        [HttpGet("{id}")]
        public ActionResult<EventModel> GetEventById(Guid id)
        {
            var _event = _eventService.GetEvent(id);
            if (_event is not null)
                return Ok(_event);
            return NotFound();
        }

        [HttpPost]
        public IActionResult Post([FromBody] CreateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            return Ok(_eventService.CreateEvent(dto));
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UpdateEventDto eventDto) 
        {
            if (!ModelState.IsValid)
                return BadRequest();
            try
            {
                _eventService.UpdateEvent(id, eventDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var isDeletedSuccessfully = _eventService.DeleteEvent(id);
            if (isDeletedSuccessfully)
                return Ok();
            return NotFound();
        }
    }
}
