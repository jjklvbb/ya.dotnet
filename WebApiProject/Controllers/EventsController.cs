using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApiProject.Interfaces;
using WebApiProject.Models;

namespace WebApiProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController (IEventService _eventService) : ControllerBase
    {
        //GET /events — получить список всех событий;
        [HttpGet]
        public IActionResult GetAllEvents()
        {
            var result = new ApiResult<Dictionary<Guid, Event>>
            {
                Data = _eventService.GetAllEvents(),
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
            try
            {
                var result = new ApiResult<Event>
                {
                    Data = _eventService.GetEventById(id),
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Получение события по ID"
                };

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                var badResult = new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Событие не найдено. Подробности: {ex.Message}"
                };

                return NotFound(badResult);
            }
        }

        //POST /events — создать событие, возвращать корректный HTTP-ответ(например, 201);
        [HttpPost]
        public IActionResult Post([FromBody]EventDTO newEvent)
        {
            if (newEvent.StartAt >= newEvent.EndAt)
            {
                ModelState.AddModelError("EndAt", "EndAt должен быть позже StartAt.");
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                    .ToArray();

                var badResult = new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}"
                };

                return BadRequest(badResult);
            }
                

            var ev = new Event(Guid.NewGuid(), newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt);

            _eventService.CreateEvent(ev);

            var result = new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Создание события"
            };

            return CreatedAtAction(nameof(GetEventById), new { id = ev.Id }, result);
        }

        //PUT /events/{id} — обновить событие целиком; если не найдено — вернуть корректный HTTP-ответ (например, 404);
        [HttpPut("{id:Guid}")]
        public IActionResult Put(Guid id, [FromBody] EventDTO newEvent)
        {
            if (newEvent.StartAt >= newEvent.EndAt)
            {
                ModelState.AddModelError("EndAt", "EndAt должен быть позже StartAt.");
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                    .ToArray();

                var badResult = new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}"
                };

                return BadRequest(badResult);
            }

            var ev = new Event(id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt);

            try
            {
                _eventService.UpdateEvent(id, ev);

                var result = new ApiResult
                {
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Обновление события"
                };

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                var badResult = new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Событие не найдено. Подробности: {ex.Message}"
                };

                return NotFound(badResult);
            }
        }

        //DELETE /events/{id} — удалить событие; если не найдено — вернуть корректный HTTP-ответ (например, 404).
        [HttpDelete("{id:Guid}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                _eventService.DeleteEvent(id);

                var result = new ApiResult
                {
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Удаление события"
                };

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                var badResult = new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Событие не найдено. Подробности: {ex.Message}"
                };

                return NotFound(badResult);
            }
        }
    }
}
