using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Events
{
    public class CreateEventDto
    {
        [MinLength(1, ErrorMessage = "Title cannot be empty")]
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required DateTime StartAt { get; set; }
        public required DateTime EndAt { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "TotalSeats is required and must be positive")]
        public required int TotalSeats { get; set; }
    }
}