using OrderManagement.Data;
using OrderManagement.Interfaces;
using OrderManagement.Models;

namespace OrderManagement.Services;

// ПРОБЛЕМА: Класс сам создаёт свои зависимости (жёсткая связанность)
public class OrderService
{
    // Зависимости создаются внутри класса
    private readonly IOrderRepository _repository = new OrderRepository();
    private readonly IEmailService _emailService = new EmailService();

    public OrderService(IOrderRepository repository, IEmailService service)
    {
        _repository = repository;
        _emailService = service;
    }

    public Order? GetOrder(int id)
    {
        return _repository.GetById(id);
    }

    public List<Order> GetAllOrders()
    {
        return _repository.GetAll();
    }

    public void CreateOrder(string customerName, decimal totalAmount)
    {
        var order = new Order
        {
            CustomerName = customerName,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.Now,
            Status = OrderStatus.Pending
        };

        _repository.Add(order);

        // Отправка уведомления
        _emailService.SendEmail(
            "customer@example.com",
            "Заказ создан",
            $"Ваш заказ #{order.Id} на сумму {order.TotalAmount} руб. создан"
        );

        Console.WriteLine($"✅ Заказ #{order.Id} успешно создан");
    }

    public void ConfirmOrder(int orderId)
    {
        var order = _repository.GetById(orderId);
        if (order == null)
        {
            Console.WriteLine($"❌ Заказ #{orderId} не найден");
            return;
        }

        order.Status = OrderStatus.Confirmed;
        _repository.Update(order);

        // Отправка уведомления
        _emailService.SendEmail(
            "customer@example.com",
            "Заказ подтверждён",
            $"Ваш заказ #{order.Id} подтверждён и будет отправлен"
        );

        Console.WriteLine($"✅ Заказ #{orderId} подтверждён");
    }
}
