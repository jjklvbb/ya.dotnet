using EventApi.Application.Ports;
using EventApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Infrastructure.DataAccess;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
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