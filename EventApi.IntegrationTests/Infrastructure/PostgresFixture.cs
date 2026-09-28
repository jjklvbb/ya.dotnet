using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WebApiProject.DataAccess;

namespace EventApi.IntegrationTests.Infrastructure;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("postgres:16-alpine")
            .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new AppDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();

        await context.Database.ExecuteSqlRawAsync(
            "DROP SCHEMA IF EXISTS public CASCADE;");

        await context.Database.ExecuteSqlRawAsync(
            "CREATE SCHEMA public;");

        await context.Database.MigrateAsync();
    }
}