using Application.DTO.Events;

namespace Application.DTO.Events
{
    public class PaginatedResultDto
    {
        public int total { get; set; }
        public List<EventDto> events { get; set; } = new List<EventDto>();
        public int currentPage { get; set; }
        public int pageSize { get; set; }
    }
}
