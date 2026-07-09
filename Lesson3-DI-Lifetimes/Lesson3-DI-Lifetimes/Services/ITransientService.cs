namespace Lesson3_DI_Lifetimes.Services;

public interface ITransientService
{
    string GetInstanceId();
    void DoWork();
}
