using EventApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using WebApiProject.DataAccess;
using WebApiProject.Entities;

namespace EventApi.IntegrationTests;

[Collection(DatabaseCollection.Name)]
public class EventRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;

    private static readonly DateTime BaseDate =
        new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public EventRepositoryTests(PostgresFixture fixture)
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
    public async Task AddAsync_AndSaveChangesAsync_PersistsEvent()
    {
        // Arrange
        var expected = CreateEvent(
            "C# Conference",
            BaseDate);

        await using (var context = _fixture.CreateContext())
        {
            var repository = new EventRepository(context);

            // Act
            await repository.AddAsync(expected);
            await repository.SaveChangesAsync();
        }

        // Assert
        await using var verificationContext = _fixture.CreateContext();

        var actual = await verificationContext.Events
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal("C# Conference", actual.Title);
        Assert.Equal(10, actual.TotalSeats);
        Assert.Equal(10, actual.AvailableSeats);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEvent()
    {
        // Arrange
        var expected = CreateEvent(
            "ASP.NET Conference",
            BaseDate);

        await SeedAsync(expected);

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        // Act
        var actual = await repository.GetByIdAsync(expected.Id);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        // Arrange
        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        // Act
        var actual = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsTrackedEventChanges()
    {
        // Arrange
        var eventEntity = CreateEvent(
            "Old title",
            BaseDate);

        await SeedAsync(eventEntity);

        var newStart = BaseDate.AddDays(5);
        var newEnd = newStart.AddHours(3);

        await using (var context = _fixture.CreateContext())
        {
            var repository = new EventRepository(context);

            var trackedEvent =
                await repository.GetByIdAsync(eventEntity.Id);

            Assert.NotNull(trackedEvent);

            // Act
            trackedEvent.Update(
                "New title",
                "New description",
                newStart,
                newEnd);

            await repository.SaveChangesAsync();
        }

        // Assert
        await using var verificationContext = _fixture.CreateContext();

        var actual = await verificationContext.Events
            .AsNoTracking()
            .SingleAsync(e => e.Id == eventEntity.Id);

        Assert.Equal("New title", actual.Title);
        Assert.Equal("New description", actual.Description);
        Assert.Equal(newStart, actual.StartAt);
        Assert.Equal(newEnd, actual.EndAt);
    }

    [Fact]
    public async Task Remove_AndSaveChangesAsync_DeletesEvent()
    {
        // Arrange
        var eventEntity = CreateEvent(
            "Event to delete",
            BaseDate);

        await SeedAsync(eventEntity);

        await using (var context = _fixture.CreateContext())
        {
            var repository = new EventRepository(context);

            var trackedEvent =
                await repository.GetByIdAsync(eventEntity.Id);

            Assert.NotNull(trackedEvent);

            // Act
            repository.Remove(trackedEvent);
            await repository.SaveChangesAsync();
        }

        // Assert
        await using var verificationContext = _fixture.CreateContext();

        var exists = await verificationContext.Events
            .AsNoTracking()
            .AnyAsync(e => e.Id == eventEntity.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task GetEventsAsync_FiltersByTitle()
    {
        // Arrange
        var first = CreateEvent(
            "C# Conference",
            BaseDate);

        var second = CreateEvent(
            "c# Meetup",
            BaseDate.AddDays(1));

        var third = CreateEvent(
            "Java Conference",
            BaseDate.AddDays(2));

        await SeedAsync(first, second, third);

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetEventsAsync(
            "C#",
            null,
            null,
            1,
            10);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.CurrentPageItems);

        var ids = result.Items
            .Select(e => e.Id)
            .ToList();

        Assert.Contains(first.Id, ids);
        Assert.Contains(second.Id, ids);
        Assert.DoesNotContain(third.Id, ids);
    }

    [Fact]
    public async Task GetEventsAsync_FiltersByFrom()
    {
        // Arrange
        var first = CreateEvent(
            "First",
            BaseDate);

        var second = CreateEvent(
            "Second",
            BaseDate.AddDays(10));

        var third = CreateEvent(
            "Third",
            BaseDate.AddDays(20));

        await SeedAsync(first, second, third);

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetEventsAsync(
            null,
            BaseDate.AddDays(10),
            null,
            1,
            10);

        // Assert
        Assert.Equal(2, result.TotalItems);

        var ids = result.Items
            .Select(e => e.Id)
            .ToList();

        Assert.DoesNotContain(first.Id, ids);
        Assert.Contains(second.Id, ids);
        Assert.Contains(third.Id, ids);
    }

    [Fact]
    public async Task GetEventsAsync_FiltersByTo()
    {
        // Arrange
        var first = CreateEvent(
            "First",
            BaseDate);

        var second = CreateEvent(
            "Second",
            BaseDate.AddDays(10));

        var third = CreateEvent(
            "Third",
            BaseDate.AddDays(20));

        await SeedAsync(first, second, third);

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetEventsAsync(
            null,
            null,
            BaseDate.AddDays(10).AddHours(2),
            1,
            10);

        // Assert
        Assert.Equal(2, result.TotalItems);

        var ids = result.Items
            .Select(e => e.Id)
            .ToList();

        Assert.Contains(first.Id, ids);
        Assert.Contains(second.Id, ids);
        Assert.DoesNotContain(third.Id, ids);
    }

    [Fact]
    public async Task GetEventsAsync_AppliesAllFilters()
    {
        // Arrange
        var tooEarly = CreateEvent(
            "Dotnet Early",
            BaseDate);

        var expected = CreateEvent(
            "Dotnet Conference",
            BaseDate.AddDays(10));

        var wrongTitle = CreateEvent(
            "Java Conference",
            BaseDate.AddDays(10));

        var tooLate = CreateEvent(
            "Dotnet Late",
            BaseDate.AddDays(30));

        await SeedAsync(
            tooEarly,
            expected,
            wrongTitle,
            tooLate);

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetEventsAsync(
            "dotnet",
            BaseDate.AddDays(5),
            BaseDate.AddDays(20),
            1,
            10);

        // Assert
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(expected.Id, result.Items.Single().Id);
    }

    [Fact]
    public async Task GetEventsAsync_AppliesPaginationAndSortsByStartAtDescending()
    {
        // Arrange
        var first = CreateEvent(
            "First",
            BaseDate.AddDays(1));

        var second = CreateEvent(
            "Second",
            BaseDate.AddDays(2));

        var third = CreateEvent(
            "Third",
            BaseDate.AddDays(3));

        var fourth = CreateEvent(
            "Fourth",
            BaseDate.AddDays(4));

        var fifth = CreateEvent(
            "Fifth",
            BaseDate.AddDays(5));

        await SeedAsync(
            first,
            second,
            third,
            fourth,
            fifth);

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetEventsAsync(
            null,
            null,
            null,
            2,
            2);

        // Assert
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(2, result.CurrentPageItems);
        Assert.Equal(5, result.TotalItems);

        Assert.Equal(
            new[] { third.Id, second.Id },
            result.Items.Select(e => e.Id));
    }

    private static Event CreateEvent(
        string title,
        DateTime startAt,
        int totalSeats = 10)
    {
        return new Event(
            Guid.NewGuid(),
            title,
            $"{title} description",
            startAt,
            startAt.AddHours(2),
            totalSeats);
    }

    private async Task SeedAsync(params Event[] events)
    {
        await using var context = _fixture.CreateContext();

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();
    }
}