using Event.Application.Common.Caching;
using Event.Application.Common.Interfaces;
using Event.Infrastructure.Caching;
using Event.Infrastructure.DataAccess;
using Event.Infrastructure.Messaging;
using Event.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Event.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var redisSection = configuration.GetSection(EventCacheOptions.SectionName);
        var redisConnectionString = redisSection["ConnectionString"];
        if (string.IsNullOrWhiteSpace(redisConnectionString))
            throw new InvalidOperationException("Redis:ConnectionString is required.");

        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;

        services.AddDbContext<EventDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisOptions));
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddOptions<EventCacheOptions>()
            .Bind(redisSection)
            .Validate(options => options.EventTtlMinutes > 0,
                "Redis:EventTtlMinutes must be greater than zero.")
            .Validate(options => options.TopEventsTtlMinutes > 0,
                "Redis:TopEventsTtlMinutes must be greater than zero.")
            .ValidateOnStart();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<BookingConfirmedProcessor>();
        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BootstrapServers),
                "Kafka:BootstrapServers is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConsumerGroup),
                "Kafka:ConsumerGroup is required.")
            .ValidateOnStart();
        services.AddHostedService<KafkaTopicInitializer>();
        services.AddHostedService<BookingConfirmedConsumer>();
        return services;
    }
}
