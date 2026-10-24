using EventApi.Application.DTOs;
using EventApi.Domain.Entities;
using EventApi.Domain.Exceptions;
using EventApi.Application.Ports;
using EventApi.Application.Interfaces;

namespace EventApi.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<PagedResult<Event>> GetEventsAsync(
            string? title,
            DateTime? from,
            DateTime? to,
            int page = 1,
            int pageSize = 10)
        {
            return await _eventRepository.GetEventsAsync(
                title,
                from,
                to,
                page,
                pageSize);
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            return await _eventRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(
                    $"Событие по ключу {id} не найдено.");
        }

        public async Task<Event> CreateEventAsync(
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt,
            int totalSeats)
        {
            var newEvent = new Event(
                Guid.NewGuid(),
                title,
                description,
                startAt,
                endAt,
                totalSeats);

            await _eventRepository.AddAsync(newEvent);
            await _eventRepository.SaveChangesAsync();

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
                ?? throw new NotFoundException(
                    $"Событие по ключу {id} не найдено.");

            existingEvent.Update(
                title,
                description,
                startAt,
                endAt);

            await _eventRepository.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(Guid id)
        {
            var existingEvent = await _eventRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(
                    $"Событие по ключу {id} не найдено.");

            _eventRepository.Remove(existingEvent);

            await _eventRepository.SaveChangesAsync();
        }
    }
}