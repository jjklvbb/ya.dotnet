using OrderManagement.Interfaces;
using OrderManagement.Models;

namespace OrderManagement.Data;

// ПРОБЛЕМА: Конкретная реализация без интерфейса
public class OrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public OrderRepository()
    {
        // Имитация данных из БД
        _orders.Add(new Order 
        { 
            Id = 1, 
            CustomerName = "Иван Иванов", 
            TotalAmount = 5000, 
            CreatedAt = DateTime.Now.AddDays(-2),
            Status = OrderStatus.Pending
        });
        _orders.Add(new Order 
        { 
            Id = 2, 
            CustomerName = "Мария Петрова", 
            TotalAmount = 12000, 
            CreatedAt = DateTime.Now.AddDays(-1),
            Status = OrderStatus.Confirmed
        });
    }

    public Order? GetById(int id)
    {
        Console.WriteLine($"[OrderRepository] Получение заказа #{id} из базы данных");
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    public List<Order> GetAll()
    {
        Console.WriteLine("[OrderRepository] Получение всех заказов из базы данных");
        return _orders;
    }

    public void Add(Order order)
    {
        order.Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1;
        _orders.Add(order);
        Console.WriteLine($"[OrderRepository] Заказ #{order.Id} добавлен в базу данных");
    }

    public void Update(Order order)
    {
        var existing = _orders.FirstOrDefault(o => o.Id == order.Id);
        if (existing != null)
        {
            existing.CustomerName = order.CustomerName;
            existing.TotalAmount = order.TotalAmount;
            existing.Status = order.Status;
            Console.WriteLine($"[OrderRepository] Заказ #{order.Id} обновлён в базе данных");
        }
    }
}
