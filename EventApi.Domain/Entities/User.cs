using EventApi.Domain.Exceptions;

namespace EventApi.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Login { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }

    private User() { } // Для EF Core

    public User(
        string login,
        string passwordHash,
        UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ValidationException(
                "Логин не может быть пустым.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ValidationException(
                "Хеш пароля не может быть пустым.");

        if (!Enum.IsDefined(role))
            throw new ValidationException(
                "Указана недопустимая роль.");

        Id = Guid.NewGuid();
        Login = login.Trim();
        PasswordHash = passwordHash;
        Role = role;
    }
}