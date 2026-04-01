namespace MovieSearch.Application.Models;

public class MovieDetailsDto
{
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!; // "movie" ili "tv"
    public string Title { get; set; } = default!;
    public string Overview { get; set; } = default!;
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string? ReleaseDate { get; set; }
    public double? Rating { get; set; }
    public IEnumerable<string> Genres { get; set; } = [];
    public IEnumerable<TrailerDto> Trailers { get; set; } = [];
}