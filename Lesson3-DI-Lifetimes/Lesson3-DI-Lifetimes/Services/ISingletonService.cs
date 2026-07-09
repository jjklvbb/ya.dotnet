namespace Lesson3_DI_Lifetimes.Services;

public interface ISingletonService
{
    string GetInstanceId();
    void DoWork();
    int GetCallCount();
}
