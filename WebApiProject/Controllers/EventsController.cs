using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApiProject.Interfaces;
using WebApiProject.Models;
using static System.Net.WebRequestMethods;

namespace WebApiProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController (IEventService _eventService) : ControllerBase
    {
        //GET /events — получить список всех событий;
        [HttpGet]
        public ApiResult<Dictionary<Guid,Event>> GetAllEvents()
        {
            return new ApiResult<Dictionary<Guid, Event>>
            {
                Data = _eventService.GetAllEvents(),
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Получение списка всех событий"
            };
        }

        //GET /events/{id} — получить событие по id; если не найдено — вернуть корректный HTTP - ответ(например, 404);
        [HttpGet("{id:Guid}")]
        public ApiBaseResult GetEventById(Guid id)
        {
            try
            {
                return new ApiResult<Event>
                {
                    Data = _eventService.GetEventById(id),
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Получение события по ID"
                };
            }
            catch (KeyNotFoundException ex)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Событие не найдено. Подробности: {ex.Message}"
                };
            }
        }

        //POST /events — создать событие, возвращать корректный HTTP-ответ(например, 201);
        [HttpPost]
        public ApiBaseResult Post([FromBody]EventDTO newEvent)
        {
            if (newEvent.StartAt > newEvent.EndAt)
            {
                ModelState.AddModelError("EndAt", "EndAt должен быть позже StartAt.");
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                    .ToArray();

                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}"
                };
            }
                

            var ev = new Event(Guid.NewGuid(), newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt);

            _eventService.CreateEvent(ev);

            return new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Создание события"
            };
        }

        //PUT /events/{id} — обновить событие целиком; если не найдено — вернуть корректный HTTP-ответ (например, 404);
        [HttpPut("{id:Guid}")]
        public ApiBaseResult Put(Guid id, [FromBody] EventDTO newEvent)
        {
            if (newEvent.StartAt > newEvent.EndAt)
            {
                ModelState.AddModelError("EndAt", "EndAt должен быть позже StartAt.");
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                    .ToArray();

                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Модель не валидна. Подробности: {string.Join("; ", errorMessages)}"
                };
            }

            var ev = new Event(id, newEvent.Title, newEvent.Description, newEvent.StartAt, newEvent.EndAt);

            try
            {
                _eventService.UpdateEvent(id, ev);

                return new ApiResult
                {
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Обновление события"
                };
            }
            catch (KeyNotFoundException ex)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Событие не найдено. Подробности: {ex.Message}"
                };
            }
        }

        //DELETE /events/{id} — удалить событие; если не найдено — вернуть корректный HTTP-ответ (например, 404).
        [HttpDelete("{id:Guid}")]
        public ApiBaseResult Delete(Guid id)
        {
            try
            {
                _eventService.DeleteEvent(id);

                return new ApiResult
                {
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Удаление события"
                };
            }
            catch (KeyNotFoundException ex)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Событие не найдено. Подробности: {ex.Message}"
                };
            }
        }
    }
}
