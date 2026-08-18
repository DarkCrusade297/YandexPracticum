using Application.Services;
using Application.Services.BookingService;
using Application.Services.EventService;
using Application.Services.PasswordService;
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
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddHostedService<BookingProcessorService>();
            return services;
        }
    }
}
