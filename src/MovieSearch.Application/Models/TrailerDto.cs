namespace MovieSearch.Application.Models;

public class TrailerDto
{
    public string Provider { get; set; } = default!; // "YouTube", "Vimeo", "TMDb"
    public string Name { get; set; } = default!;
    public string Url { get; set; } = default!;
}