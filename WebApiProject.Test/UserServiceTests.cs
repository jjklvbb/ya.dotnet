
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventApi.Application.Services;
using EventApi.Domain.Entities;
using EventApi.Domain.Exceptions;
using EventApi.Infrastructure.DataAccess;
using EventApi.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace WebApiProject.Test;

public class UserServiceTests
{
    private const string Secret =
        "test-secret-key-at-least-32-characters-long";

    private static (
        AppDbContext Context,
        UserService Service,
        IdentityPasswordHasher Hasher) CreateSut()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var hasher = new IdentityPasswordHasher();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = Secret,
                ["Jwt:Issuer"] = "EventApi",
                ["Jwt:Audience"] = "EventApiClient",
                ["Jwt:LifetimeMinutes"] = "30"
            })
            .Build();

        var service = new UserService(
            new UserRepository(context),
            hasher,
            new JwtTokenService(configuration));

        return (context, service, hasher);
    }

    [Fact]
    public async Task Register_SavesHashedPasswordAndDefaultRole()
    {
        var (context, service, hasher) = CreateSut();
        await using var db = context;

        await service.RegisterAsync("Katya", "Password123!");

        var user = await context.Users.SingleAsync();

        Assert.Equal("katya", user.Login);
        Assert.Equal(UserRole.User, user.Role);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.True(hasher.Verify("Password123!", user.PasswordHash));
    }

    [Fact]
    public async Task Login_ReturnsValidJwtWithAdminRole()
    {
        var (context, service, _) = CreateSut();
        await using var db = context;

        await service.RegisterAsync(
            "Admin", "Password123!", UserRole.Admin);

        var token = await service.LoginAsync(
            "ADMIN", "Password123!");

        var handler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };

        var principal = handler.ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "EventApi",
                ValidateAudience = true,
                ValidAudience = "EventApiClient",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Secret)),
                ClockSkew = TimeSpan.Zero
            },
            out var validatedToken);

        var user = await context.Users.SingleAsync();

        Assert.IsType<JwtSecurityToken>(validatedToken);
        Assert.Equal(
            user.Id.ToString(),
            principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        Assert.Equal(
            "Admin",
            principal.FindFirstValue(ClaimTypes.Role));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsValidationException()
    {
        var (context, service, _) = CreateSut();
        await using var db = context;

        await service.RegisterAsync("katya", "Password123!");

        await Assert.ThrowsAsync<ValidationException>(
            () => service.LoginAsync("katya", "WrongPassword"));
    }

    [Fact]
    public async Task Register_WithDuplicateLogin_ThrowsValidationException()
    {
        var (context, service, _) = CreateSut();
        await using var db = context;

        await service.RegisterAsync("Katya", "Password123!");

        await Assert.ThrowsAsync<ValidationException>(
            () => service.RegisterAsync("KATYA", "Password123!"));
    }
}
