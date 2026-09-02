namespace Event.Application.Common.Caching;

public sealed class EventCacheOptions
{
    public const string SectionName = "Redis";

    public int EventTtlMinutes { get; init; } = 5;
    public int TopEventsTtlMinutes { get; init; } = 5;
}
