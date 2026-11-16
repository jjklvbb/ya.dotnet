using EventApi.Application.Ports;
using Microsoft.AspNetCore.Identity;

namespace EventApi.Infrastructure.Security;

public sealed class IdentityPasswordHasher : IUserPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    private readonly object _user = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(_user, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(
            _user,
            passwordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}