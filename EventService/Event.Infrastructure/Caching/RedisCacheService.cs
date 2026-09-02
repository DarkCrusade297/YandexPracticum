using Event.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Event.Infrastructure.Caching;

public sealed class RedisCacheService(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            return value.IsNull ? null : value.ToString();
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Could not read Redis cache key {CacheKey}", key);
            return null;
        }
    }

    public async Task SetAsync(string key, string value, TimeSpan expiration)
    {
        try
        {
            await _database.StringSetAsync(key, value, expiration);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Could not write Redis cache key {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Could not remove Redis cache key {CacheKey}", key);
        }
    }
}
