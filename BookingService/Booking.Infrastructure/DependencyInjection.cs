using Booking.Application.Common.Interfaces;
using Booking.Infrastructure.DataAccess;
using Booking.Infrastructure.Gateways;
using Booking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BookingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddHttpClient<IEventGateway, EventHttpGateway>(client =>
            client.BaseAddress = new Uri(configuration["Services:EventService"]
                ?? throw new InvalidOperationException("Services:EventService is not configured.")));
        return services;
    }
}
