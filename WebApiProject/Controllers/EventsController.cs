using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApiProject.Entities;
using WebApiProject.Interfaces;
using WebApiProject.DTOs;
using WebApiProject.Responses;
using System.ComponentModel.DataAnnotations;

namespace WebApiProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public EventsController(
            IEventService eventService,
            IBookingService bookingService)
        {
            _eventService = eventService;
            _bookingService = bookingService;
        }

        //GET /events — получить список событий (с поддержкой фильтрации и пагинации);
        [HttpGet]
        public IActionResult GetEvents(
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

                throw new Exceptions.ValidationException(
                    $"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}");
            }

            var result = new ApiResult<PagedResult<Event>>
            {
                Data = _eventService.GetEvents(filter, page, pageSize),
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Получение списка всех событий"
            };

            return Ok(result);
        }

        //GET /events/{id} — получить событие по id; если не найдено — вернуть корректный HTTP - ответ(например, 404);
        [HttpGet("{id:Guid}")]
        public IActionResult GetEventById(Guid id)
        {
            Event ev = _eventService.GetEventById(id);

            var result = new ApiResult<Event>
                {
                    Data = ev,
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Получение события по ID"
                };

            return Ok(result);
        }

        //POST /events — создать событие, возвращать корректный HTTP-ответ(например, 201);
        [HttpPost]
        public IActionResult Post([FromBody]EventDTO newEvent)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                    .ToArray();

                throw new Exceptions.ValidationException($"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}");
            }

            var ev = new Event(Guid.NewGuid(), newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt);

            _eventService.CreateEvent(ev);

            var result = new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.Created,
                Message = "Создание события"
            };

            return CreatedAtAction(nameof(GetEventById), new { id = ev.Id }, result);
        }

        //PUT /events/{id} — обновить событие целиком; если не найдено — вернуть корректный HTTP-ответ (например, 404);
        [HttpPut("{id:Guid}")]
        public IActionResult Put(Guid id, [FromBody] EventDTO newEvent)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                    .ToArray();

                throw new Exceptions.ValidationException($"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}");
            }

            var ev = new Event(id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt);

            _eventService.UpdateEvent(id, ev);

            var result = new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Обновление события"
            };

            return Ok(result);
            
        }

        //DELETE /events/{id} — удалить событие; если не найдено — вернуть корректный HTTP-ответ (например, 404).
        [HttpDelete("{id:Guid}")]
        public IActionResult Delete(Guid id)
        {
            _eventService.DeleteEvent(id);

            return NoContent();
        }

        [HttpPost("{id:guid}/book")]
        public async Task<IActionResult> CreateBooking(Guid id)
        {
            var booking = await _bookingService.CreateBookingAsync(id);

            var result = new ApiResult<BookingInfo>
            {
                Data = booking,
                Success = true,
                StatusCode = HttpStatusCode.Accepted,
                Message = "Бронь создана и ожидает обработки"
            };

            return AcceptedAtAction(
                "GetBookingById",
                "Bookings",
                new { id = booking.Id },
                result);
        }
    }
}
