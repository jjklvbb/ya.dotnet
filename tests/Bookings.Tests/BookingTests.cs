using Bookings.Domain.Entities;
using Bookings.Domain.Exceptions;

namespace Bookings.Tests;

public class BookingTests
{
    [Fact]
    public void NewBooking_HasPendingStatus()
    {
        var booking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Null(booking.ProcessedAt);
    }

    [Fact]
    public void Confirm_ChangesStatusToConfirmed()
    {
        var booking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid());

        booking.Confirm();

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    [Fact]
    public void Confirm_Twice_ThrowsValidationException()
    {
        var booking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid());

        booking.Confirm();

        Assert.Throws<ValidationException>(
            () => booking.Confirm());
    }

    [Fact]
    public void Reject_ChangesStatusToRejected()
    {
        var booking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid());

        booking.Reject();

        Assert.Equal(BookingStatus.Rejected, booking.Status);
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled()
    {
        var booking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid());

        booking.Confirm();
        booking.Cancel();

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void CreateBooking_WithEmptyEventId_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(
            () => new Booking(
                Guid.Empty,
                Guid.NewGuid()));
    }

    [Fact]
    public void Cancel_Twice_ThrowsValidationException()
    {
        var booking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid());

        booking.Cancel();

        Assert.Throws<ValidationException>(
            () => booking.Cancel());
    }
}