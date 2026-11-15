namespace Messaging.Contracts.Events;

public sealed record BookingConfirmed(
    Guid BookingId,
    Guid EventId);