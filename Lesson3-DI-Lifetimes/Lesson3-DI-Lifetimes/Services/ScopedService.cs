namespace Lesson3_DI_Lifetimes.Services;

public class ScopedService : IScopedService, IDisposable
{
    private readonly string _instanceId;
    private readonly ILogger<ScopedService> _logger;
    
    public ScopedService(ILogger<ScopedService> logger)
    {
        _instanceId = Guid.NewGuid().ToString()[..8];
        _logger = logger;
        _logger.LogInformation("[SCOPED] Создан: {Id}", _instanceId);
    }
    
    public string GetInstanceId() => _instanceId;
    
    public void DoWork()
    {
        _logger.LogInformation("[SCOPED] Работает: {Id}", _instanceId);
    }
    
    public void Dispose()
    {
        _logger.LogInformation("[SCOPED] Освобождён: {Id}", _instanceId);
    }
}
