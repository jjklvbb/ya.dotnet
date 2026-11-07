using EventApi.Application.Interfaces;
using EventApi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiProject.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly IUserService _users;

    public AuthController(IUserService users)
    {
        _users = users;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        await _users.RegisterAsync(
            request.Login,
            request.Password,
            request.Role,
            ct);

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var token = await _users.LoginAsync(
            request.Login,
            request.Password,
            ct);

        return Ok(new { accessToken = token });
    }
}

public record RegisterRequest(
    string Login,
    string Password,
    UserRole Role = UserRole.User);

public record LoginRequest(
    string Login,
    string Password);