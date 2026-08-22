using Microsoft.Extensions.DependencyInjection;

namespace User.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserApplication(this IServiceCollection services)
    {
        return services;
    }
}
