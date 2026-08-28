using System.ComponentModel.DataAnnotations;

namespace Event.Application.DTO;

public class UpdateEventDto : IValidatableObject
{
    [Required, MinLength(1)]
    public string? Title { get; set; }
    [MaxLength(2000)]
    public string? Description { get; set; }
    [Required]
    public DateTime? StartAt { get; set; }
    [Required]
    public DateTime? EndAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt.HasValue && EndAt.HasValue && EndAt.Value <= StartAt.Value)
            yield return new ValidationResult("EndDate must be greater than StartDate", [nameof(EndAt)]);
    }
}
