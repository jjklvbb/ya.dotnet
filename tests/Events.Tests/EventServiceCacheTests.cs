using Events.Application.Caching;
using Events.Application.DTOs;
using Events.Application.Ports;
using Events.Application.Services;
using Events.Domain.Entities;

namespace Events.Tests;

public class EventServiceCacheTests
{
    private static readonly DateTime Start = new(2026, 12, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = Start.AddHours(2);
    private static readonly EventsCacheSettings Settings = new(
        TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(30));

    [Fact]
    public async Task GetById_CacheHit_DoesNotCallRepository()
    {
        var ev = NewEvent();
        var repository = new FakeRepository { Event = ev };
        var cache = new FakeCache();
        var expected = EventResponseDto.FromEntity(ev);
        cache.Values[EventCacheKeys.ById(ev.Id)] = expected;
        var service = new EventService(repository, cache, Settings);

        var result = await service.GetEventByIdAsync(ev.Id);

        Assert.Equal(expected, result);
        Assert.Equal(0, repository.GetByIdCalls);
        Assert.Equal(0, cache.SetCalls);
    }

    [Fact]
    public async Task GetById_CacheMiss_LoadsAndCachesEvent()
    {
        var ev = NewEvent();
        var repository = new FakeRepository { Event = ev };
        var cache = new FakeCache();
        var service = new EventService(repository, cache, Settings);

        var result = await service.GetEventByIdAsync(ev.Id);

        Assert.Equal(ev.Id, result.Id);
        Assert.Equal(ev.AvailableSeats, result.AvailableSeats);
        Assert.Equal(1, repository.GetByIdCalls);
        Assert.Equal(1, cache.SetCalls);
        Assert.Equal(Settings.EventTtl, cache.LastTtl);
        Assert.True(cache.Values.ContainsKey(EventCacheKeys.ById(ev.Id)));
    }

    [Fact]
    public async Task GetTopEvents_CacheHit_DoesNotCallRepository()
    {
        var repository = new FakeRepository();
        var cache = new FakeCache();
        var expected = new List<EventResponseDto> { EventResponseDto.FromEntity(NewEvent()) };
        cache.Values[EventCacheKeys.Top10] = expected;
        var service = new EventService(repository, cache, Settings);

        var result = await service.GetTopEventsAsync();

        Assert.Single(result);
        Assert.Equal(expected[0], result[0]);
        Assert.Equal(0, repository.GetTopCalls);
    }

    [Fact]
    public async Task GetTopEvents_CacheMiss_LoadsAndCachesTopTen()
    {
        var ev = NewEvent();
        var repository = new FakeRepository { TopEvents = [ev] };
        var cache = new FakeCache();
        var service = new EventService(repository, cache, Settings);

        var result = await service.GetTopEventsAsync();

        Assert.Single(result);
        Assert.Equal(1, repository.GetTopCalls);
        Assert.Equal(10, repository.LastRequestedTopCount);
        Assert.Equal(Settings.TopEventsTtl, cache.LastTtl);
        Assert.True(cache.Values.ContainsKey(EventCacheKeys.Top10));
    }

    [Fact]
    public async Task UpdateEvent_InvalidatesAfterSaving()
    {
        var ev = NewEvent();
        var repository = new FakeRepository { Event = ev };
        var cache = new FakeCache();
        repository.TrackCacheRemoval(cache);
        var service = new EventService(repository, cache, Settings);

        await service.UpdateEventAsync(ev.Id, "Новое название", null, Start, End);

        Assert.Equal("Новое название", ev.Title);
        Assert.Equal(1, repository.SaveCalls);
        Assert.Equal(EventCacheKeys.ById(ev.Id), cache.LastRemovedKey);
        Assert.True(repository.WasSavedWhenCacheRemoved(cache));
    }

    [Fact]
    public async Task DeleteEvent_InvalidatesAfterSaving()
    {
        var ev = NewEvent();
        var repository = new FakeRepository { Event = ev };
        var cache = new FakeCache();
        repository.TrackCacheRemoval(cache);
        var service = new EventService(repository, cache, Settings);

        await service.DeleteEventAsync(ev.Id);

        Assert.True(repository.WasRemoved);
        Assert.Equal(1, repository.SaveCalls);
        Assert.Equal(EventCacheKeys.ById(ev.Id), cache.LastRemovedKey);
        Assert.True(repository.WasSavedWhenCacheRemoved(cache));
    }

    private static Event NewEvent() => new(Guid.NewGuid(), "Концерт", null, Start, End, 20);

    private sealed class FakeCache : ICacheService
    {
        public Dictionary<string, object> Values { get; } = new();
        public int SetCalls { get; private set; }
        public TimeSpan? LastTtl { get; private set; }
        public string? LastRemovedKey { get; private set; }
        public Action? OnRemove { get; set; }

        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult(Values.TryGetValue(key, out var value) ? (T?)value : default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            Values[key] = value!;
            SetCalls++;
            LastTtl = ttl;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            OnRemove?.Invoke();
            LastRemovedKey = key;
            Values.Remove(key);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRepository : IEventRepository
    {
        public Event? Event { get; set; }
        public List<Event> TopEvents { get; set; } = [];
        public int GetByIdCalls { get; private set; }
        public int GetTopCalls { get; private set; }
        public int LastRequestedTopCount { get; private set; }
        public int SaveCalls { get; private set; }
        public bool WasRemoved { get; private set; }

        public bool WasSavedWhenCacheRemoved(FakeCache cache)
        {
            // Для проверки порядка выполняемая инвалидация должна застать SaveChanges уже вызванным.
            return _saveHadCompletedAtRemoval;
        }

        private bool _saveHadCompletedAtRemoval;
        public void TrackCacheRemoval(FakeCache cache)
        {
            cache.OnRemove = () => _saveHadCompletedAtRemoval = SaveCalls > 0;
        }

        public Task<PagedResult<Event>> GetEventsAsync(
            string? title, DateTime? from, DateTime? to, int page, int pageSize,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            GetByIdCalls++;
            return Task.FromResult(Event?.Id == id ? Event : null);
        }

        public Task<List<Event>> GetTopEventsAsync(int count, CancellationToken cancellationToken = default)
        {
            GetTopCalls++;
            LastRequestedTopCount = count;
            return Task.FromResult(TopEvents);
        }

        public Task AddAsync(Event newEvent, CancellationToken cancellationToken = default)
        {
            Event = newEvent;
            return Task.CompletedTask;
        }

        public void Remove(Event eventEntity) => WasRemoved = true;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.CompletedTask;
        }
    }
}
