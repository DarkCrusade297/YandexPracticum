namespace Event.Application.Common.Caching;

public static class EventCacheKeys
{
    public const string Top10 = "events:top10";

    public static string ById(Guid id) => $"event:{id}";
}
