namespace WebApiProject.Test;

using WebApiProject.Exceptions;
using WebApiProject.Models;
using WebApiProject.Services;

public class EventServiceTest
{
    private readonly EventService _eventService;
    private readonly List<Event> _events;

    public EventServiceTest()
    {
        _events =
        [
            new() { Id = new Guid("2eff557e-fc03-4ca7-b95d-64146130d992"), Title = "Концерт Димы Билана", Description = "...", 
                StartAt = new DateTime(2026,10,31,19,0,0), EndAt = new DateTime(2026,10,31,22,0,0)},
            new() { Id = new Guid("4708d2ee-a407-48a9-82f5-47c9743f8ccf"), Title = "Встреча с одногруппниками", Description = "...",
                StartAt = new DateTime(2026,8,11,12,0,0), EndAt = new DateTime(2026,8,11,16,0,0)},
            new() { Id = new Guid("d1b5b26a-0136-42c5-ac2f-9734ef8aff61"), Title = "Тимбилдинг", Description = "...",
                StartAt = new DateTime(2026,7,23,19,0,0), EndAt = new DateTime(2026,7,23,23,0,0)},
        ];

        _eventService = new EventService(_events);
    }

    // ==========================================
    // УСПЕШНЫЕ СЦЕНАРИИ
    // ==========================================

    [Fact]
    public void CreateEvent_ValidEvent_AddsToCollection() //создание события;
    {
        // Arrange
        var newId = Guid.NewGuid();
        var newEvent = new Event(newId, "Новое событие", "Описание", DateTime.Now.AddDays(1), DateTime.Now.AddDays(2));

        // Act
        _eventService.CreateEvent(newEvent);
        var result = _eventService.GetEventById(newId);

        // Assert
        Assert.Equal("Новое событие", result.Title);
    }

    [Fact]
    public void GetEvents_NoFilter_ReturnsAllEvents() //получение всех событий;
    {
        // Arrange
        var filter = new EventFilterParameters();

        // Act
        var result = _eventService.GetEvents(filter);

        // Assert
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.Items.Count());
    }

    [Fact]
    public void GetEventById_ExistingId_ReturnsCorrectEvent() //получение события по ID;
    {
        // Arrange
        var targetId = _events[0].Id;

        // Act
        var result = _eventService.GetEventById(targetId);

        // Assert
        Assert.Equal("Концерт Димы Билана", result.Title);
    }

    [Fact]
    public void UpdateEvent_ExistingEvent_UpdatesSuccessfully() //обновление существующего события;
    {
        // Arrange
        var idToUpdate = _events[0].Id;
        var updatedEvent = new Event(idToUpdate, "Новое название", "Обновленное описание",
            new DateTime(2026, 11, 1), new DateTime(2026, 11, 2));

        // Act
        _eventService.UpdateEvent(idToUpdate, updatedEvent);
        var result = _eventService.GetEventById(idToUpdate);

        // Assert
        Assert.Equal("Новое название", result.Title);
    }

    [Fact]
    public void DeleteEvent_ExistingEvent_DeletesSuccessfully()
    {
        // Arrange
        var idToDelete = _events[1].Id;

        // Act
        _eventService.DeleteEvent(idToDelete);

        // Assert
        Assert.Throws<NotFoundException>(() => _eventService.GetEventById(idToDelete));
    }

    [Fact]
    public void GetEvents_WithTitleFilter_ReturnsMatchingEventsCaseInsensitive() //фильтрация по названию;
    {
        // Arrange
        var filter = new EventFilterParameters { Title = "концерт" }; // Маленькие буквы

        // Act
        var result = _eventService.GetEvents(filter);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Концерт Димы Билана", result.Items.First().Title);
    }

    [Fact]
    public void GetEvents_WithDateFilter_ReturnsMultipleMatchingEvents()
    {
        // Arrange
        var filter = new EventFilterParameters
        {
            From = new DateTime(2026, 7, 1),
            To = new DateTime(2026, 8, 31) 
        };

        // Act
        var result = _eventService.GetEvents(filter);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count());

        var titles = result.Items.Select(e => e.Title).ToList();
        Assert.Contains("Встреча с одногруппниками", titles);
        Assert.Contains("Тимбилдинг", titles);
    }

    [Fact]
    public void GetEvents_WithPagination_ReturnsCorrectPageAndTotals() //пагинация событий;
    {
        // Arrange
        var filter = new EventFilterParameters();
        int page = 1;
        int pageSize = 2;

        // Act
        var result = _eventService.GetEvents(filter, page, pageSize);

        // Assert
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(2, result.CurrentPageItems);
        Assert.Equal(3, result.TotalItems);
    }

    [Fact]
    public void GetEvents_WithCombinedFilter_ReturnsMatchingEvents() //комбинированная фильтрация.
    {
        // Arrange
        var filter = new EventFilterParameters
        {
            Title = "концерт",
            From = new DateTime(2026, 10, 1),
            To = new DateTime(2026, 12, 31)
        };

        // Act
        var result = _eventService.GetEvents(filter);

        // Assert
        Assert.Single(result.Items);
    }

    // ==========================================
    // НЕУСПЕШНЫЕ СЦЕНАРИИ
    // ==========================================

    [Fact]
    public void GetEventById_NonExistingId_ThrowsNotFoundException() //попытка получить событие с несуществующим ID;
    {
        // Arrange
        var fakeId = Guid.NewGuid();

        // Act
        Action act = () => _eventService.GetEventById(fakeId);

        // Assert
        Assert.Throws<NotFoundException>(act);
    }

    [Fact]
    public void UpdateEvent_NonExistingId_ThrowsNotFoundException() //попытка обновить событие с несуществующим ID;
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        var fakeEvent = new Event(fakeId, "Title", "Desc", DateTime.Now, DateTime.Now.AddDays(1));

        // Act
        Action act = () => _eventService.UpdateEvent(fakeId, fakeEvent);

        // Assert
        Assert.Throws<NotFoundException>(act);
    }
}
