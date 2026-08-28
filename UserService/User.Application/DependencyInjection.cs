using Microsoft.Extensions.DependencyInjection;
using User.Application.Services;

namespace User.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, Services.UserService>();
        services.AddScoped<IPasswordService, PasswordService>();
        return services;
    }
}
