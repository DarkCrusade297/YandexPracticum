using Application.DTO.Bookings;
using Application.Services.BookingService;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventManagerSystem.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(ILogger<BookingController> logger, IBookingService bookingService)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<GetBookingDto?>> GetBookingById(Guid id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var booking = await _bookingService.GetBookingByIdAsync(id, userId, userRole);
            return Ok(booking);
        }

        [HttpPost("/events/{id}/book")]
        [Authorize]
        [ProducesResponseType(typeof(CreatedBookingDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CreatedBookingDto>> CreateBookingByEventId(Guid id)
        {
            var userId = GetCurrentUserId();

            var bk = await _bookingService.CreateBookingAsync(id, userId)
                ?? throw new InvalidOperationException("Booking was not created.");

            var locationUri = Url.Action(
                nameof(GetBookingById),
                new { id = bk.Id }
            );

            Response.Headers.Location = locationUri;
            return Accepted(bk);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            await _bookingService.CancelBookingAsync(id, userId, userRole);

            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private UserRoles GetCurrentUserRole()
        {
            return Enum.Parse<UserRoles>(User.FindFirstValue(ClaimTypes.Role)!);
        }
    }
}
