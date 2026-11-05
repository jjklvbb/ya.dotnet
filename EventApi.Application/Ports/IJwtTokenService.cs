using EventApi.Domain.Entities;

namespace EventApi.Application.Ports;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}