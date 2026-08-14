using Application.Services;
using Application.Services.BookingService;
using Application.Services.EventService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IEventService, EventService>();
            services.AddHostedService<BookingProcessorService>();
            return services;
        }
    }
}
