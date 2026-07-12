using System.ComponentModel.DataAnnotations;

namespace WebApiProject.Models
{
    public class Event
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public void Validate()
        {
            if (StartAt > EndAt)
                throw new ArgumentException("EndAt должен быть позже StartAt.");
        }
    }
}
