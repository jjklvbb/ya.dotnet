namespace Events.Application.Caching;

public static class EventCacheKeys
{
    public static string ById(Guid id) => $"event:{id}";

    public const string Top10 = "events:top10";
}
