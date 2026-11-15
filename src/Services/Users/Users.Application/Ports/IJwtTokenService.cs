using Users.Domain.Entities;

namespace Users.Application.Ports;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}