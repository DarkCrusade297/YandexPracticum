using System.ComponentModel.DataAnnotations;

namespace EventManagerSystem.DTO.Events
{
    public class UpdateEventDto : IValidatableObject
    {
        [Required(ErrorMessage = "Title field is required")]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "StartAt field is required")]
        public DateTime? StartAt { get; set; }
        [Required(ErrorMessage = "EndAt field is required")]
        public DateTime? EndAt { get; set; }
        [Required(ErrorMessage = "TotalSeats field is required")]
        public int? TotalSeats { get; set; }
        public int? AvailableSeats { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartAt >= EndAt)
            {
                yield return new ValidationResult(errorMessage: "EndDate must be greater than StartDate", memberNames: new[] { nameof(EndAt) });
            }
            if (TotalSeats < 1)
            {
                yield return new ValidationResult(errorMessage: "TotalSets must be greater than 0");
            }
            if (TotalSeats < AvailableSeats)
            {
                yield return new ValidationResult(errorMessage: "TotalSets must be greater than AvailableSeats");
            }
        }
    }
}
