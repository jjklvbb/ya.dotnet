using WebApiProject.DTOs;
using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;

namespace WebApiProject.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<PagedResult<Event>> GetEventsAsync(
            EventFilterParameters filter,
            int page = 1,
            int pageSize = 10)
        {
            return await _eventRepository.GetEventsAsync(
                filter.Title,
                filter.From,
                filter.To,
                page,
                pageSize);
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            return await _eventRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(
                    $"Событие по ключу {id} не найдено.");
        }

        public async Task CreateEventAsync(Event newEvent)
        {
            await _eventRepository.AddAsync(newEvent);
            await _eventRepository.SaveChangesAsync();
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