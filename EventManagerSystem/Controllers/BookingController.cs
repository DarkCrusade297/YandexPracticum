using Application.DTO.Bookings;
using Application.Services.BookingService;
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
        [ProducesResponseType(typeof(CreatedBookingDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CreatedBookingDto>> CreateBookingByEventId(Guid id)
        {
            var bk = await _bookingService.CreateBookingAsync(id)
                ?? throw new InvalidOperationException("Booking was not created.");

            var locationUri = Url.Action(
                nameof(GetBookingById),
                new { id = bk.Id }
            );

            Response.Headers.Location = locationUri;
            return Accepted(bk);
        }
    }
}
