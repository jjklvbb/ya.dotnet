using System.ComponentModel.DataAnnotations;

namespace WebApiProject.Models
{
    public class Event
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public Event() { }

        public Event(Guid id, string title, string description, DateTime startAt, DateTime endAt)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
    }
}
