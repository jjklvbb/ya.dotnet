namespace Lesson3_DI_Lifetimes.Services;

public interface IScopedService
{
    string GetInstanceId();
    void DoWork();
}
