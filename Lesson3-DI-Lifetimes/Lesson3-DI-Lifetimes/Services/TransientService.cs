namespace Lesson3_DI_Lifetimes.Services;

public class TransientService : ITransientService, IDisposable
{
    private readonly string _instanceId;
    private readonly ILogger<TransientService> _logger;
    
    public TransientService(ILogger<TransientService> logger)
    {
        _instanceId = Guid.NewGuid().ToString()[..8];
        _logger = logger;
        _logger.LogInformation("[TRANSIENT] Создан: {Id}", _instanceId);
    }
    
    public string GetInstanceId() => _instanceId;
    
    public void DoWork()
    {
        _logger.LogInformation("[TRANSIENT] Работает: {Id}", _instanceId);
    }
    
    public void Dispose()
    {
        _logger.LogInformation("[TRANSIENT] Освобождён: {Id}", _instanceId);
    }
}
