using Microsoft.AspNetCore.Mvc;
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
        public ActionResult<Dictionary<Guid,Event>> GetAllEvents()
        {
            return _eventService.GetAllEvents();
        }

        //GET /events/{id} — получить событие по id; если не найдено — вернуть корректный HTTP - ответ(например, 404);
        [HttpGet("{id:Guid}")]
        public ActionResult<Event> GetEventById(Guid id)
        {
            try
            {
                return _eventService.GetEventById(id);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound($"Событие не найдено: {ex.Message}");
            }
        }

        //POST /events — создать событие, возвращать корректный HTTP-ответ(например, 201);
        [HttpPost]
        public IActionResult Post([FromBody]Event newEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _eventService.CreateEvent(newEvent);

            return Created();
        }

        //PUT /events/{id} — обновить событие целиком; если не найдено — вернуть корректный HTTP-ответ (например, 404);
        [HttpPut("{id:Guid}")]
        public IActionResult Put(Guid id, [FromBody] Event newEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _eventService.UpdateEvent(id, newEvent);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound($"Событие не найдено: {ex.Message}");
            }
        }

        //DELETE /events/{id} — удалить событие; если не найдено — вернуть корректный HTTP-ответ (например, 404).
        [HttpDelete("{id:Guid}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                _eventService.DeleteEvent(id);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound($"Событие не найдено: {ex.Message}");
            }
        }
    }
}
