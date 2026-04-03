namespace MovieSearch.Application.Models;

public class MovieSearchResultDto
{
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
    public IEnumerable<MovieSummaryDto> Results { get; set; } = [];
}