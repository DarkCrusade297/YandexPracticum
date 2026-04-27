using EventManagerSystem.Models;

namespace EventManagerSystem.DTO
{
    public class PaginatedResultDto
    {
        public int total { get; set; }
        public List<EventModel> events { get; set; }
        public int currentPage { get; set; }
        public int pageSize { get; set; }
    }
}
