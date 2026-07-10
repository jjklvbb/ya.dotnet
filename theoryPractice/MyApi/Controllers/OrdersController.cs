using Microsoft.AspNetCore.Mvc;
using MyApi.Models;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        // Имитация базы данных
        private static List<Order> _orders = new()
        {
            new Order { Id = 1, CustomerName = "Иван Иванов", TotalAmount = 1500.50m, OrderDate = DateTime.Now.AddDays(-5) },
            new Order { Id = 2, CustomerName = "Мария Петрова", TotalAmount = 2300.00m, OrderDate = DateTime.Now.AddDays(-2) }
        };

        /// <summary>
        /// Получить все заказы
        /// </summary>
        /// <returns>Список всех заказов</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_orders);
        }

        /// <summary>
        /// Получить заказ по ID
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <returns>Заказ с указанным ID</returns>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound($"Заказ с ID {id} не найден");
            }

            return Ok(order);
        }

        /// <summary>
        /// Создать новый заказ
        /// </summary>
        /// <param name="order">Данные заказа</param>
        /// <returns>Созданный заказ</returns>
        [HttpPost]
        public IActionResult Create([FromBody] Order order)
        {
            order.Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1;
            order.OrderDate = DateTime.Now;
            _orders.Add(order);

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        /// <summary>
        /// Обновить существующий заказ
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="order">Новые данные заказа</param>
        /// <returns>Обновлённый заказ</returns>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Order order)
        {
            var existingOrder = _orders.FirstOrDefault(o => o.Id == id);
            if (existingOrder == null)
            {
                return NotFound($"Заказ с ID {id} не найден");
            }

            existingOrder.CustomerName = order.CustomerName;
            existingOrder.TotalAmount = order.TotalAmount;

            return Ok(existingOrder);
        }

        /// <summary>
        /// Удалить заказ
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <returns>Статус удаления</returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound($"Заказ с ID {id} не найден");
            }

            _orders.Remove(order);
            return NoContent();
        }
    }
}
