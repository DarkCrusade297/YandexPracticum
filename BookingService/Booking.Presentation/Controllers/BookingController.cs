using Booking.Application.DTO.Bookings;
using Booking.Application.Services;
using Booking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booking.Presentation.Controllers;

[ApiController]
[Route("bookings")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpGet("{id:guid}"), Authorize]
    public async Task<ActionResult<GetBookingDto?>> GetBookingById(Guid id) =>
        Ok(await bookingService.GetBookingByIdAsync(id, GetCurrentUserId(), GetCurrentUserRole()));

    [HttpPost("/events/{id:guid}/book"), Authorize]
    public async Task<ActionResult<CreatedBookingDto>> CreateBookingByEventId(Guid id)
    {
        var booking = await bookingService.CreateBookingAsync(id, GetCurrentUserId())
            ?? throw new InvalidOperationException("Booking was not created.");
        Response.Headers.Location = Url.Action(nameof(GetBookingById), new { id = booking.Id });
        return Accepted(booking);
    }

    [HttpDelete("{id:guid}"), Authorize]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        await bookingService.CancelBookingAsync(id, GetCurrentUserId(), GetCurrentUserRole());
        return NoContent();
    }

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRoles GetCurrentUserRole() => Enum.Parse<UserRoles>(User.FindFirstValue(ClaimTypes.Role)!);
}
