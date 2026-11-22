using Events.Application.Caching;
using Events.Application.DTOs;
using Events.Application.Interfaces;
using Events.Application.Ports;
using Events.Domain.Entities;
using Events.Domain.Exceptions;

namespace Events.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cache;
    private readonly EventsCacheSettings _cacheSettings;

    public EventService(
        IEventRepository eventRepository,
        ICacheService cache,
        EventsCacheSettings cacheSettings)
    {
        _eventRepository = eventRepository;
        _cache = cache;
        _cacheSettings = cacheSettings;
    }

    public Task<PagedResult<Event>> GetEventsAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page = 1,
        int pageSize = 10)
    {
        return _eventRepository.GetEventsAsync(title, from, to, page, pageSize);
    }

    public async Task<EventResponseDto> GetEventByIdAsync(Guid id)
    {
        string key = EventCacheKeys.ById(id);

        var cached = await _cache.GetAsync<EventResponseDto>(key);
        if (cached is not null)
            return cached;

        var ev = await _eventRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Событие по ключу {id} не найдено.");

        var result = EventResponseDto.FromEntity(ev);
        await _cache.SetAsync(key, result, _cacheSettings.EventTtl);
        return result;
    }

    public async Task<IReadOnlyList<EventResponseDto>> GetTopEventsAsync()
    {
        var cached = await _cache.GetAsync<List<EventResponseDto>>(EventCacheKeys.Top10);
        if (cached is not null)
            return cached;

        var events = await _eventRepository.GetTopEventsAsync(10);
        var result = events.Select(EventResponseDto.FromEntity).ToList();
        await _cache.SetAsync(EventCacheKeys.Top10, result, _cacheSettings.TopEventsTtl);
        return result;
    }

    public async Task<Event> CreateEventAsync(
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats)
    {
        var newEvent = new Event(
            Guid.NewGuid(), title, description, startAt, endAt, totalSeats);

        await _eventRepository.AddAsync(newEvent);
        await _eventRepository.SaveChangesAsync();

        // Новый объект ещё не находится в кеше; топ обновится по TTL.
        return newEvent;
    }

    public async Task UpdateEventAsync(
        Guid id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt)
    {
        var existingEvent = await _eventRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Событие по ключу {id} не найдено.");

        existingEvent.Update(title, description, startAt, endAt);
        await _eventRepository.SaveChangesAsync();
        await _cache.RemoveAsync(EventCacheKeys.ById(id));
    }

    public async Task DeleteEventAsync(Guid id)
    {
        var existingEvent = await _eventRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Событие по ключу {id} не найдено.");

        _eventRepository.Remove(existingEvent);
        await _eventRepository.SaveChangesAsync();
        await _cache.RemoveAsync(EventCacheKeys.ById(id));
    }
}
