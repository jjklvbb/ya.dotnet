var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

// Minimal API эндоинты
app.MapGet("/api/products/minimal", () => "Products from Minimal API");
app.MapGet("/api/products/minimal/{id}", (int id) => $"Product {id} from Minimal API");

var productsGroup = app.MapGroup("/api/products/group");
productsGroup.MapGet("/", () => "Products group");
productsGroup.MapGet("/{id}", (int id) => $"Product {id} from group");

app.Run();

// Не удалять! Без этого тесты не будут работать корректно
// Тесты используют WebApplicationFactory<Program> для создания тестового сервера
// и проверки работы маршрутов. Класс Program должен быть доступен для тестов.
public partial class Program { }
