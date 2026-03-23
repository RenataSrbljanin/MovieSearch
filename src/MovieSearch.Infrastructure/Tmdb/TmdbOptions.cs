namespace MovieSearch.Infrastructure.Tmdb;

public class TmdbOptions
{
    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3/";
    public string ApiToken { get; set; } = default!;

}
