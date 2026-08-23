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

        public Event() { }

        public Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название не может быть пустым", nameof(title));

            if (startAt >= endAt)
                throw new ArgumentException("Дата начала должна быть строго раньше даты окончания", nameof(startAt));

            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
    }
}
