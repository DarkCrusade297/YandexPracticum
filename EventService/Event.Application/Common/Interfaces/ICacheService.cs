namespace Event.Application.Common.Interfaces;

public interface ICacheService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan expiration);
    Task RemoveAsync(string key);
}
