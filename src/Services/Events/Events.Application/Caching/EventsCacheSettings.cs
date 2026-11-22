namespace Events.Application.Caching;

public sealed record EventsCacheSettings(
    TimeSpan EventTtl,
    TimeSpan TopEventsTtl);
