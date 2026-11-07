using EventApi.Domain.Exceptions;

namespace EventApi.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public Guid UserId { get; private set; }
        public User? User { get; private set; }
        public Event Event { get; private set; } = null!;
        public BookingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        private Booking() { }

        public Booking(Guid eventId, Guid userId)
        {
            if (eventId == Guid.Empty)
                throw new ValidationException(
                    "Идентификатор события не может быть пустым.");

            if (userId == Guid.Empty)
                throw new ValidationException(
                    "Идентификатор пользователя не может быть пустым.");

            Id = Guid.NewGuid();
            EventId = eventId;
            UserId = userId;
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void Confirm()
        {
            if (Status != BookingStatus.Pending)
                throw new ValidationException(
                    "Подтвердить можно только ожидающую бронь.");

            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            if (Status != BookingStatus.Pending)
                throw new ValidationException(
                    "Отклонить можно только ожидающую бронь.");

            Status = BookingStatus.Rejected;
            ProcessedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == BookingStatus.Cancelled)
                throw new ValidationException("Бронь уже отменена.");

            if (Status == BookingStatus.Rejected)
                throw new ValidationException("Отклонённую бронь нельзя отменить.");

            Status = BookingStatus.Cancelled;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
