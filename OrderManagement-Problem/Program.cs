using OrderManagement.Data;
using OrderManagement.Interfaces;
using OrderManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// ПРОБЛЕМА: Зависимости не регистрируются в DI-контейнере
// OrderService сам создаёт свои зависимости
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

// Эндпоинты
app.MapGet("/orders", (OrderService orderService) =>
{
    var orders = orderService.GetAllOrders();
    return Results.Ok(orders);
});

app.MapGet("/orders/{id:int}", (int id, OrderService orderService) =>
{
    var order = orderService.GetOrder(id);
    return order != null ? Results.Ok(order) : Results.NotFound();
});

app.MapPost("/orders", (CreateOrderRequest request, OrderService orderService) =>
{
    orderService.CreateOrder(request.CustomerName, request.TotalAmount);
    return Results.Ok(new { message = "Заказ создан" });
});

app.MapPost("/orders/{id:int}/confirm", (int id, OrderService orderService) =>
{
    orderService.ConfirmOrder(id);
    return Results.Ok(new { message = "Заказ подтверждён" });
});

app.Run();

record CreateOrderRequest(string CustomerName, decimal TotalAmount);
