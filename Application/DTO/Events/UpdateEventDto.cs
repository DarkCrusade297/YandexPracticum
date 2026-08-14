using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Events
{
    public class UpdateEventDto : IValidatableObject
    {
        [Required(ErrorMessage = "Title field is required")]
        [MinLength(1, ErrorMessage = "Title cannot be empty")]
        public string? Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description is too long")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "StartAt field is required")]
        public DateTime? StartAt { get; set; }

        [Required(ErrorMessage = "EndAt field is required")]
        public DateTime? EndAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartAt.HasValue && EndAt.HasValue && EndAt.Value <= StartAt.Value)
            {
                yield return new ValidationResult(
                    "EndDate must be greater than StartDate",
                    new[] { nameof(EndAt) });
            }
        }
    }
}
