public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next; // 1. Ссылка на следующий middleware

    // 2. Конструктор — получаем следующий middleware из DI-контейнера
    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Логирование метода и пути
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Request: {context.Request.Method} {context.Request.Path}");

        // Добавление заголовка с временем обработки
        context.Response.Headers.Add("X-Custom-Header", $"MyApp");

        // Передача управления следующему middleware
        await _next(context);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Response: {context.Response.StatusCode}");
    }
}