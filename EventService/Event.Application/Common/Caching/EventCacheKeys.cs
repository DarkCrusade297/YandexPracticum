namespace Event.Application.Common.Caching;

public static class EventCacheKeys
{
    public static string ById(Guid id) => $"events:{id}";
}
