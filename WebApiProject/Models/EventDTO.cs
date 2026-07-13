using System.ComponentModel.DataAnnotations;

namespace WebApiProject.Models
{
    public class EventDTO
    {
        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public EventDTO() { }

        public EventDTO(string title, string desciption, DateTime startAt, DateTime endAt)
        {
            Title = title;
            Description = desciption;
            StartAt = startAt;
            EndAt = endAt;
        }

    }
}
