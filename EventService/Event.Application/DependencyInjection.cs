using Microsoft.Extensions.DependencyInjection;
using Event.Application.Services;

namespace Event.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEventApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, Services.EventService>();
        return services;
    }
}
