using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Application.Common.Interfaces;
using User.Application.Common.Settings;
using User.Application.Services;
using User.Infrastructure.DataAccess;
using User.Infrastructure.Repositories;
using User.Infrastructure.Security;
using System.Text;

namespace User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Secret), "JWT secret is required.")
            .Validate(settings => Encoding.UTF8.GetByteCount(settings.Secret) >= 32, "JWT secret must be at least 32 bytes.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Issuer), "JWT issuer is required.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Audience), "JWT audience is required.")
            .Validate(settings => settings.ExpirationMinutes > 0, "JWT expiration must be greater than zero.")
            .ValidateOnStart();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        return services;
    }
}
