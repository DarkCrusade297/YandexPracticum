using Booking.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Presentation.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception at {Path}", context.Request.Path);
            var status = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                NoAvailableSeatsException or BookingCancelException or BookingLimitExceededException => StatusCodes.Status409Conflict,
                ForbiddenOperationException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Detail = ex.Message });
        }
    }
}
