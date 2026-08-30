using System.ComponentModel.DataAnnotations;

namespace WebApiProject.DTOs
{
    public class EventDTO : IValidatableObject
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public EventDTO() { }

        public EventDTO(string title, string? description, DateTime startAt, DateTime endAt)
        {
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndAt <= StartAt)
            {
                yield return new ValidationResult(
                    "EndAt должен быть позже StartAt",
                    new[] { nameof(EndAt) });
            }
        }

    }
}
