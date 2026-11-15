using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Users.Domain.Entities;

namespace Users.Infrastructure.DataAccess;

public sealed class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(UsersDbContext).Assembly);
    }
}