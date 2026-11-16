using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(DatabaseCollection.Name)]
public class MigrationTests
{
    private readonly PostgresFixture _fixture;

    public MigrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Migrate_CreatesExpectedTables()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();

        // Act
        var tables = await context.Database
            .SqlQueryRaw<string>(
                """
                SELECT table_name AS "Value"
                FROM information_schema.tables
                WHERE table_schema = 'public'
                """)
            .ToListAsync();

        // Assert
        Assert.Contains("events", tables);
        Assert.Contains("bookings", tables);
        Assert.Contains("__EFMigrationsHistory", tables);
    }
}