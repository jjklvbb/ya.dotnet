using EventApi.Application.Interfaces;
using EventApi.Application.Ports;
using EventApi.Domain.Entities;
using EventApi.Domain.Exceptions;

namespace EventApi.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IUserPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public UserService(
        IUserRepository users,
        IUserPasswordHasher hasher,
        IJwtTokenService jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task RegisterAsync(
        string login,
        string password,
        UserRole role = UserRole.User,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(login) ||
            login.Trim().Length > 100)
        {
            throw new ValidationException(
                "Логин должен содержать от 1 до 100 символов.");
        }

        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < 8)
        {
            throw new ValidationException(
                "Пароль должен содержать минимум 8 символов.");
        }

        var existingUser = await _users.GetByLoginAsync(
            login,
            cancellationToken);

        if (existingUser is not null)
        {
            throw new ValidationException(
                "Пользователь с таким логином уже существует.");
        }

        var passwordHash = _hasher.Hash(password);

        var user = new User(login, passwordHash, role);

        await _users.AddAsync(user, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> LoginAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = string.IsNullOrWhiteSpace(login)
            ? null
            : await _users.GetByLoginAsync(login, cancellationToken);

        if (user is null ||
            !_hasher.Verify(password, user.PasswordHash))
        {
            throw new ValidationException(
                "Неверный логин или пароль.");
        }

        return _jwt.GenerateToken(user);
    }
}