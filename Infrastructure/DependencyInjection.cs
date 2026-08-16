using Application.Common.Interfaces;
using Application.Repositories.Booking;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Booking;
using Infrastructure.Repositories.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection")));
            // Services
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            return services;
        }
    }
}
