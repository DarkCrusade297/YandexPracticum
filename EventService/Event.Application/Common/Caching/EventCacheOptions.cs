namespace Event.Application.Common.Caching;

public sealed class EventCacheOptions
{
    public const string SectionName = "Cache";

    public int EventTtlMinutes { get; init; } = 5;
}
