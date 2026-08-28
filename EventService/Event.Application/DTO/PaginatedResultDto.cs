namespace Event.Application.DTO;

public class PaginatedResultDto
{
    public int Total { get; set; }
    public List<EventDto> Events { get; set; } = [];
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
