using EventApi.Domain.Entities;
using EventApi.Infrastructure.DataAccess;
using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EventApi.IntegrationTests;

[Collection(DatabaseCollection.Name)]
public class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    public UserRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        return _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddAsync_ShouldSaveUser()
    {
        // Arrange
        var user = new User(
            "katya",
            "test-password-hash",
            UserRole.Admin);

        await using (var context = _fixture.CreateContext())
        {
            var repository = new UserRepository(context);

            // Act
            await repository.AddAsync(user);
            await repository.SaveChangesAsync();
        }

        // Assert
        await using var verificationContext = _fixture.CreateContext();

        var actual = await verificationContext.Users
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(user.Id, actual.Id);
        Assert.Equal("katya", actual.Login);
        Assert.Equal("test-password-hash", actual.PasswordHash);
        Assert.Equal(UserRole.Admin, actual.Role);
    }

    [Theory]
    [InlineData("katya")]
    [InlineData("KATYA")]
    [InlineData("  Katya  ")]
    public async Task GetByLoginAsync_ShouldIgnoreCaseAndSpaces(
        string login)
    {
        // Arrange
        await using (var context = _fixture.CreateContext())
        {
            var repository = new UserRepository(context);

            await repository.AddAsync(
                new User("Katya", "test-hash"));

            await repository.SaveChangesAsync();
        }

        await using var searchContext = _fixture.CreateContext();
        var searchRepository = new UserRepository(searchContext);

        // Act
        var actual = await searchRepository.GetByLoginAsync(login);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("katya", actual.Login);
    }

    [Fact]
    public async Task AddAsync_ShouldRejectDuplicateLogin()
    {
        // Arrange
        await using (var context = _fixture.CreateContext())
        {
            var repository = new UserRepository(context);

            await repository.AddAsync(
                new User("Katya", "first-hash"));

            await repository.SaveChangesAsync();
        }

        await using var duplicateContext = _fixture.CreateContext();
        var duplicateRepository = new UserRepository(duplicateContext);

        // Act
        await duplicateRepository.AddAsync(
            new User("  KATYA  ", "second-hash"));

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(
            () => duplicateRepository.SaveChangesAsync());
    }
}