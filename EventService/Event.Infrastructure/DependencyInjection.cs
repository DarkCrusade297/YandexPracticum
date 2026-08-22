using Microsoft.Extensions.DependencyInjection;

namespace Event.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
