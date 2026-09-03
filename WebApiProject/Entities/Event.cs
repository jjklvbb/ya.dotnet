using System.ComponentModel.DataAnnotations;

namespace WebApiProject.Entities
{
    public class Event
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public int TotalSeats { get; private set; }

        public int AvailableSeats { get; private set; }

        public Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название не может быть пустым", nameof(title));

            if (startAt >= endAt)
                throw new ArgumentException("Дата начала должна быть строго раньше даты окончания", nameof(startAt));

            if (totalSeats <= 0)
                throw new Exceptions.ValidationException("Количество мест должно быть больше нуля.");

            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = totalSeats;
        }

        public bool TryReserveSeats(int count = 1)
        {
            if (AvailableSeats < count)
                return false;

            AvailableSeats -= count;
            return true;
        }

        public void ReleaseSeats(int count = 1)
        {
            AvailableSeats += count;

            if (AvailableSeats > TotalSeats)
                AvailableSeats = TotalSeats;
        }

        public void Update(string title, string? description, DateTime startAt, DateTime endAt)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название не может быть пустым",  nameof(title));

            if (startAt >= endAt)
                throw new ArgumentException("Дата начала должна быть строго раньше даты окончания",  nameof(startAt));

            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
    }
}
