using Booking.Application.Common.Interfaces;
using Booking.Application.DTO.Events;
using Booking.Domain.Exceptions;
using System.Net;
using System.Net.Http.Json;

namespace Booking.Infrastructure.Gateways;

public class EventHttpGateway(HttpClient client) : IEventGateway
{
    public async Task<EventDto> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync($"internal/events/{eventId}", cancellationToken);
        await EnsureSuccessAsync(response, eventId, cancellationToken);
        return await response.Content.ReadFromJsonAsync<EventDto>(cancellationToken)
            ?? throw new InvalidOperationException("Event service returned an empty response.");
    }

    public Task ReserveSeatAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        SendAsync($"internal/events/{eventId}/reserve", eventId, cancellationToken);

    public Task ReleaseSeatAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        SendAsync($"internal/events/{eventId}/release", eventId, cancellationToken);

    private async Task SendAsync(string uri, Guid eventId, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsync(uri, null, cancellationToken);
        await EnsureSuccessAsync(response, eventId, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, Guid eventId, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) throw new NotFoundException($"Event with id '{eventId}' not found");
        if (response.StatusCode == HttpStatusCode.Conflict) throw new NoAvailableSeatsException(message);
        response.EnsureSuccessStatusCode();
    }
}
