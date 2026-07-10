using Lesson3_DI_Lifetimes.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lesson3_DI_Lifetimes.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LifecycleController : ControllerBase
{
    private readonly ITransientService _transient1;
    private readonly ITransientService _transient2;
    private readonly IScopedService _scoped1;
    private readonly IScopedService _scoped2;
    private readonly ISingletonService _singleton1;
    private readonly ISingletonService _singleton2;
    private readonly ILogger<LifecycleController> _logger;
    
    public LifecycleController(
        ITransientService transient1,
        ITransientService transient2,
        IScopedService scoped1,
        IScopedService scoped2,
        ISingletonService singleton1,
        ISingletonService singleton2,
        ILogger<LifecycleController> logger)
    {
        _transient1 = transient1;
        _transient2 = transient2;
        _scoped1 = scoped1;
        _scoped2 = scoped2;
        _singleton1 = singleton1;
        _singleton2 = singleton2;
        _logger = logger;
        
        _logger.LogInformation(" LifecycleController создан");
    }
    
    [HttpGet("test")]
    public IActionResult TestLifecycles()
    {
        _logger.LogInformation(" Начало запроса /api/lifecycle/test ===");
        
        // Вызываем методы всех сервисов
        _transient1.DoWork();
        _transient2.DoWork();
        _scoped1.DoWork();
        _scoped2.DoWork();
        _singleton1.DoWork();
        _singleton2.DoWork();
        
        var result = new
        {
            Transient1 = _transient1.GetInstanceId(),
            Transient2 = _transient2.GetInstanceId(),
            Scoped1 = _scoped1.GetInstanceId(),
            Scoped2 = _scoped2.GetInstanceId(),
            Singleton1 = _singleton1.GetInstanceId(),
            Singleton2 = _singleton2.GetInstanceId(),
            SingletonCallCount = _singleton1.GetCallCount()
        };
        
        _logger.LogInformation("Конец запроса /api/lifecycle/test ===");
        return Ok(result);
    }
    
    [HttpGet("simple")]
    public IActionResult SimpleTest()
    {
        _logger.LogInformation("Начало запроса /api/lifecycle/simple ===");
        
        _transient1.DoWork();
        _scoped1.DoWork();
        _singleton1.DoWork();
       
        _logger.LogInformation(" Конец запроса /api/lifecycle/simple ===");
        return Ok("Проверьте логи!");
    }
}
