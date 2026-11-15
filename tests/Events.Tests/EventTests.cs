using Events.Domain.Entities;
using Events.Domain.Exceptions;

namespace Events.Tests;

public class EventTests
{
    private static Event CreateEvent(int seats = 10)
    {
        return new Event(
            Guid.NewGuid(),
            "Концерт",
            "Тестовое мероприятие",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            seats);
    }

    [Fact]
    public void NewEvent_AllSeatsAreAvailable()
    {
        var ev = CreateEvent();

        Assert.Equal(10, ev.TotalSeats);
        Assert.Equal(10, ev.AvailableSeats);
    }

    [Fact]
    public void TryReserveSeats_DecreasesAvailableSeats()
    {
        var ev = CreateEvent();

        var reserved = ev.TryReserveSeats();

        Assert.True(reserved);
        Assert.Equal(9, ev.AvailableSeats);
    }

    [Fact]
    public void TryReserveSeats_WhenNoSeats_ReturnsFalse()
    {
        var ev = CreateEvent(1);

        Assert.True(ev.TryReserveSeats());
        Assert.False(ev.TryReserveSeats());

        Assert.Equal(0, ev.AvailableSeats);
    }

    [Fact]
    public void ReleaseSeats_CannotExceedTotalSeats()
    {
        var ev = CreateEvent(1);

        ev.TryReserveSeats();
        ev.ReleaseSeats();
        ev.ReleaseSeats();

        Assert.Equal(1, ev.AvailableSeats);
    }

    [Fact]
    public void CreateEvent_WithZeroSeats_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(
            () => CreateEvent(0));
    }
}