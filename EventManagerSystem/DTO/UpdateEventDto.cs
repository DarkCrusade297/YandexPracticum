using System.ComponentModel.DataAnnotations;

namespace EventManagerSystem.DTO
{
    public class UpdateEventDto
    {
        [Required(ErrorMessage = "Title field is required")]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "StartAt field is required")]
        public DateTime? StartAt { get; set; }
        [Required(ErrorMessage = "EndAt field is required")]
        public DateTime? EndAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartAt >= EndAt)
            {
                yield return new ValidationResult(errorMessage: "EndDate must be greater than StartDate", memberNames: new[] { nameof(EndAt) });
            }
        }
    }
}
