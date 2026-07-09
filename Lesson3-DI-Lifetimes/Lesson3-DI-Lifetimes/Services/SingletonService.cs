namespace Lesson3_DI_Lifetimes.Services;

public class SingletonService : ISingletonService, IDisposable
{
    private readonly string _instanceId;
    private readonly ILogger<SingletonService> _logger;
    private int _callCount = 0;
    
    public SingletonService(ILogger<SingletonService> logger)
    {
        _instanceId = Guid.NewGuid().ToString()[..8];
        _logger = logger;
        _logger.LogInformation("[SINGLETON] Создан: {Id}", _instanceId);
    }
    
    public string GetInstanceId() => _instanceId;
    
    public void DoWork()
    {
        _callCount++;
        _logger.LogInformation("[SINGLETON] Работает: {Id}, вызов #{Count}", _instanceId, _callCount);
    }
    
    public int GetCallCount() => _callCount;
    
    public void Dispose()
    {
        _logger.LogInformation("[SINGLETON] Освобождён: {Id}", _instanceId);
    }
}
