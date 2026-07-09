using OrderManagement.Interfaces;

namespace OrderManagement.Services;

// ПРОБЛЕМА: Конкретная реализация без интерфейса
public class EmailService : IEmailService
{
    public void SendEmail(string to, string subject, string body)
    {
        // Имитация отправки email
        Console.WriteLine($"[EmailService] Отправка email:");
        Console.WriteLine($"  Кому: {to}");
        Console.WriteLine($"  Тема: {subject}");
        Console.WriteLine($"  Сообщение: {body}");
    }
}
