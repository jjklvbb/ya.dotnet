using Users.Infrastructure.DataAccess;
using Users.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Ports;

namespace Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<IUserPasswordHasher,
            IdentityPasswordHasher>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}