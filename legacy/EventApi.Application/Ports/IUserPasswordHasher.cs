namespace EventApi.Application.Ports;

public interface IUserPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}