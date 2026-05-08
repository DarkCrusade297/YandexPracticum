using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Models;
using EventManagerSystem.Services;
using EventManagerSystem.Services.BookingService;
using EventManagerSystem.Services.EventService;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerSystem.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingController : ControllerBase
    {
        private IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(ILogger<BookingController> logger, IBookingService bookingService)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetBookingDto?>> GetBookingById(Guid id)
        {
            var ev = await _bookingService.GetBookingByIdAsync(id);
            return Ok(ev);
        }

        [HttpPost("/events/{id}/book")]
        public async Task<ActionResult<CreatedBookingDto?>> CreateBookingByEventId(Guid id)
        {
            var bk = await _bookingService.CreateBookingAsync(id);
            var locationUri = Url.Action(
                nameof(GetBookingById),
                new { id = bk.Id }
            );

            Response.Headers.Location = locationUri;
            return Accepted(bk);
        }
    }
}
