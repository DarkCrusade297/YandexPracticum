using Microsoft.Extensions.DependencyInjection;

namespace Event.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEventApplication(this IServiceCollection services)
    {
        return services;
    }
}
