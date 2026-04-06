using EventManagerSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerSystem.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private List<EventModel> events = new List<EventModel>();
        private readonly ILogger<EventsController> _logger;

        public EventsController(ILogger<EventsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<EventModel> GetAllEvents()
        {
            return (IEnumerable<EventModel>)Ok(events);
        }

        [HttpGet("{id}")]
        public IEnumerable<EventModel> GetEventById(Guid id)
        {
            return (IEnumerable<EventModel>)Ok(events[0]);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EventModel model)
        {
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] EventModel model) 
        { 
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Ok();
        }
    }
}
