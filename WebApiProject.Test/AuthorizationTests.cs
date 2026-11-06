
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using EventApi.Domain.Entities;
using EventApi.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WebApiProject.Controllers;
using Microsoft.Extensions.Hosting;

namespace WebApiProject.Test;

public class AuthorizationTests
{
    private const string Secret =
        "test-secret-key-at-least-32-characters-long";

    private static async Task<IHost> CreateHostAsync()
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();

                webHost.ConfigureServices(services =>
                {
                    services.AddControllers()
                        .AddApplicationPart(typeof(EventsController).Assembly);

                    services
                        .AddAuthentication(
                            JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.MapInboundClaims = false;

                            options.TokenValidationParameters =
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
                                };
                        });

                    services.AddAuthorization();
                });

                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                });
            })
            .Build();

        await host.StartAsync();

        return host;
    }

    private static string CreateToken(UserRole role)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = Secret,
                ["Jwt:Issuer"] = "EventApi",
                ["Jwt:Audience"] = "EventApiClient",
                ["Jwt:LifetimeMinutes"] = "30"
            })
            .Build();

        var user = new User("test-user", "test-hash", role);

        return new JwtTokenService(configuration)
            .GenerateToken(user);
    }

    [Theory]
    [InlineData("POST", "/events")]
    [InlineData("PUT", "/events/11111111-1111-1111-1111-111111111111")]
    [InlineData("DELETE", "/events/11111111-1111-1111-1111-111111111111")]
    [InlineData("POST", "/events/11111111-1111-1111-1111-111111111111/book")]
    [InlineData("GET", "/bookings/11111111-1111-1111-1111-111111111111")]
    [InlineData("DELETE", "/bookings/11111111-1111-1111-1111-111111111111")]
    public async Task WithoutToken_Returns401(
        string method,
        string path)
    {
        using var host = await CreateHostAsync();
        using var client = host.GetTestClient();

        using var request = new HttpRequestMessage(
            new HttpMethod(method), path);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/events")]
    [InlineData("PUT", "/events/11111111-1111-1111-1111-111111111111")]
    [InlineData("DELETE", "/events/11111111-1111-1111-1111-111111111111")]
    public async Task RegularUser_CannotManageEvents_Returns403(
        string method,
        string path)
    {
        using var host = await CreateHostAsync();
        using var client = host.GetTestClient();

        using var request = new HttpRequestMessage(
            new HttpMethod(method), path);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", CreateToken(UserRole.User));

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
