using System.ComponentModel.DataAnnotations;

namespace WebApiProject.DTOs
{
    public class UpdateEventDTO : IValidatableObject
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public UpdateEventDTO() { }

        public UpdateEventDTO(string title, string? description, DateTime startAt, DateTime endAt)
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
                yield return new ValidationResult("EndAt должен быть позже StartAt", [nameof(EndAt)]);
            }
        }

    }
}
