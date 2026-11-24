using Microsoft.AspNetCore.Mvc;
using System.Net;
using Events.Domain.Entities;
using Events.Application.Interfaces;
using Events.Api.DTOs;
using Events.Application.DTOs;
using Events.Api.Responses;
using System.ComponentModel.DataAnnotations;
using DomainValidationException = Events.Domain.Exceptions.ValidationException;
using Microsoft.AspNetCore.Authorization;

namespace Events.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromQuery] EventFilterParameters filter,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, int.MaxValue)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errorMessages = ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                .ToArray();

            throw new DomainValidationException($"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}");
        }

        if ((filter.From.HasValue && filter.From.Value.Kind != DateTimeKind.Utc) ||
            (filter.To.HasValue && filter.To.Value.Kind != DateTimeKind.Utc))
            throw new DomainValidationException(
                "Параметры from и to должны быть указаны в UTC (с суффиксом Z).");

        var result = new ApiResult<PagedResult<Event>>
        {
            Data = await _eventService.GetEventsAsync(filter.Title, filter.From, filter.To, page, pageSize),
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Получение списка всех событий"
        };

        return Ok(result);
    }

    [HttpGet("top")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTopEvents()
    {
        var events = await _eventService.GetTopEventsAsync();
        return Ok(new ApiResult<IReadOnlyList<EventResponseDto>>
        {
            Data = events,
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Топ-10 мероприятий по доле проданных мест"
        });
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetEventById(Guid id)
    {
        var ev = await _eventService.GetEventByIdAsync(id);
        return Ok(new ApiResult<EventResponseDto>
        {
            Data = ev,
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Получение события по ID"
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] CreateEventDTO newEvent)
    {
        if (!ModelState.IsValid)
        {
            var errorMessages = ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                .ToArray();
            throw new DomainValidationException($"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}");
        }

        if (newEvent.StartAt.Kind != DateTimeKind.Utc || newEvent.EndAt.Kind != DateTimeKind.Utc)
            throw new DomainValidationException(
                "Даты StartAt и EndAt должны быть указаны в UTC (с суффиксом Z).");

        var ev = await _eventService.CreateEventAsync(
            newEvent.Title,
            newEvent.Description,
            newEvent.StartAt,
            newEvent.EndAt,
            newEvent.TotalSeats!.Value);

        var result = new ApiResult<Event>
        {
            Success = true,
            StatusCode = HttpStatusCode.Created,
            Message = "Событие создано",
            Data = ev
        };

        return CreatedAtAction(nameof(GetEventById), new { id = ev.Id }, result);
    }

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateEventDTO newEvent)
    {
        if (!ModelState.IsValid)
        {
            var errorMessages = ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                .ToArray();
            throw new DomainValidationException($"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}");
        }

        if (newEvent.StartAt.Kind != DateTimeKind.Utc || newEvent.EndAt.Kind != DateTimeKind.Utc)
            throw new DomainValidationException(
                "Даты StartAt и EndAt должны быть указаны в UTC (с суффиксом Z).");

        await _eventService.UpdateEventAsync(
            id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt);

        return Ok(new ApiResult
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Обновление события"
        });
    }

    [HttpDelete("{id:Guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _eventService.DeleteEventAsync(id);
        return NoContent();
    }
}
