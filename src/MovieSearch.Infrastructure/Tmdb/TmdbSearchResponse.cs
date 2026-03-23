namespace MovieSearch.Infrastructure.Tmdb;

public class TmdbSearchResponse
{
    public int Page { get; set; }
    public int Total_Pages { get; set; }
    public int Total_Results { get; set; }
    public List<TmdbSearchItem> Results { get; set; } = new();
}

public class TmdbSearchItem
{
    public string Media_Type { get; set; } = default!;
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Name { get; set; }
    public string? Overview { get; set; }
    public string? Poster_Path { get; set; }
    public string? Release_Date { get; set; }
    public string? First_Air_Date { get; set; }
    public double? Vote_Average { get; set; }
}