using Users.Application.Ports;
using Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Users.Infrastructure.DataAccess;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByLoginAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        var normalizedLogin = login.Trim().ToLowerInvariant();

        return _context.Users
            .FirstOrDefaultAsync(
                u => u.Login == normalizedLogin,
                cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}