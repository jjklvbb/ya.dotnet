namespace WebApiProject.Test;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApiProject.DataAccess;
using WebApiProject.DTOs;
using WebApiProject.Entities;
using WebApiProject.Exceptions;
using WebApiProject.Interfaces;
using WebApiProject.Services;

public class EventServiceTest : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IEventService _eventService;
    private readonly List<Event> _events;

    public EventServiceTest()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventService, EventService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();

        _eventService = _scope.ServiceProvider
            .GetRequiredService<IEventService>();

        var context = _scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        _events =
        [
            new Event(
                new Guid("2eff557e-fc03-4ca7-b95d-64146130d992"),
                "Концерт Димы Билана",
                "...",
                new DateTime(2026, 10, 31, 19, 0, 0),
                new DateTime(2026, 10, 31, 22, 0, 0),
                10),

            new Event(
                new Guid("4708d2ee-a407-48a9-82f5-47c9743f8ccf"),
                "Встреча с одногруппниками",
                "...",
                new DateTime(2026, 8, 11, 12, 0, 0),
                new DateTime(2026, 8, 11, 16, 0, 0),
                10),

            new Event(
                new Guid("d1b5b26a-0136-42c5-ac2f-9734ef8aff61"),
                "Тимбилдинг",
                "...",
                new DateTime(2026, 7, 23, 19, 0, 0),
                new DateTime(2026, 7, 23, 23, 0, 0),
                10)
        ];

        context.Events.AddRange(_events);
        context.SaveChanges();
    }

    // ==========================================
    // УСПЕШНЫЕ СЦЕНАРИИ
    // ==========================================

    [Fact]
    public async Task CreateEvent_ValidEvent_AddsToDatabase()
    {
        // Arrange
        var newId = Guid.NewGuid();

        var newEvent = new Event(
            newId,
            "Новое событие",
            "Описание",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(2),
            10);

        // Act
        await _eventService.CreateEventAsync(newEvent);

        var result = await _eventService.GetEventByIdAsync(newId);

        // Assert
        Assert.Equal("Новое событие", result.Title);
        Assert.Equal(10, result.TotalSeats);
        Assert.Equal(10, result.AvailableSeats);
    }

    [Fact]
    public async Task GetEvents_NoFilter_ReturnsAllEvents()
    {
        // Arrange
        var filter = new EventFilterParameters();

        // Act
        var result = await _eventService.GetEventsAsync(filter, 1, 10);

        // Assert
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.Items.Count());
    }

    [Fact]
    public async Task GetEventById_ExistingId_ReturnsCorrectEvent()
    {
        // Arrange
        var targetId = _events[0].Id;

        // Act
        var result = await _eventService.GetEventByIdAsync(targetId);

        // Assert
        Assert.Equal("Концерт Димы Билана", result.Title);
    }

    [Fact]
    public async Task UpdateEvent_ExistingEvent_UpdatesSuccessfully()
    {
        // Arrange
        var idToUpdate = _events[0].Id;

        // Act
        await _eventService.UpdateEventAsync(
            idToUpdate,
            "Новое название",
            "Обновленное описание",
            new DateTime(2026, 11, 1),
            new DateTime(2026, 11, 2));

        var result = await _eventService.GetEventByIdAsync(idToUpdate);

        // Assert
        Assert.Equal("Новое название", result.Title);
        Assert.Equal(10, result.TotalSeats);
        Assert.Equal(10, result.AvailableSeats);
    }

    [Fact]
    public async Task DeleteEvent_ExistingEvent_DeletesSuccessfully()
    {
        // Arrange
        var idToDelete = _events[1].Id;

        // Act
        await _eventService.DeleteEventAsync(idToDelete);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _eventService.GetEventByIdAsync(idToDelete));
    }

    [Fact]
    public async Task GetEvents_WithTitleFilter_ReturnsMatchingEventsCaseInsensitive()
    {
        // Arrange
        var filter = new EventFilterParameters
        {
            Title = "концерт"
        };

        // Act
        var result = await _eventService.GetEventsAsync(filter, 1, 10);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(
            "Концерт Димы Билана",
            result.Items.First().Title);
    }

    [Fact]
    public async Task GetEvents_WithDateFilter_ReturnsMultipleMatchingEvents()
    {
        // Arrange
        var filter = new EventFilterParameters
        {
            From = new DateTime(2026, 7, 1),
            To = new DateTime(2026, 8, 31)
        };

        // Act
        var result = await _eventService.GetEventsAsync(filter, 1, 10);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count());

        var titles = result.Items
            .Select(e => e.Title)
            .ToList();

        Assert.Contains("Встреча с одногруппниками", titles);
        Assert.Contains("Тимбилдинг", titles);
    }

    [Fact]
    public async Task GetEvents_WithPagination_ReturnsCorrectPageAndTotals()
    {
        // Arrange
        var filter = new EventFilterParameters();

        const int page = 1;
        const int pageSize = 2;

        // Act
        var result = await _eventService.GetEventsAsync(
            filter,
            page,
            pageSize);

        // Assert
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(2, result.CurrentPageItems);
        Assert.Equal(3, result.TotalItems);
    }

    [Fact]
    public async Task GetEvents_WithCombinedFilter_ReturnsMatchingEvents()
    {
        // Arrange
        var filter = new EventFilterParameters
        {
            Title = "концерт",
            From = new DateTime(2026, 10, 1),
            To = new DateTime(2026, 12, 31)
        };

        // Act
        var result = await _eventService.GetEventsAsync(filter, 1, 10);

        // Assert
        Assert.Single(result.Items);
    }

    // ==========================================
    // НЕУСПЕШНЫЕ СЦЕНАРИИ
    // ==========================================

    [Fact]
    public async Task GetEventById_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var fakeId = Guid.NewGuid();

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _eventService.GetEventByIdAsync(fakeId));
    }

    [Fact]
    public async Task UpdateEvent_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var fakeId = Guid.NewGuid();

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _eventService.UpdateEventAsync(
                fakeId,
                "Title",
                "Desc",
                DateTime.Now,
                DateTime.Now.AddDays(1)));
    }

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }
}