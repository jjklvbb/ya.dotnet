namespace WebApiProject.Test;

using System.Net;
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

    [Fact]
    public void Test1()
    {
        var _service = new EventService();
    }
}
