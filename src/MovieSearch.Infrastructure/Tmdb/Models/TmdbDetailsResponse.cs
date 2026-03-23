using System.Text.Json.Serialization;

namespace MovieSearch.Infrastructure.Tmdb;

public class TmdbDetailsResponse
{
    public int Id { get; set; }

    // Movie fields
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    // TV fields
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("first_air_date")]
    public string? FirstAirDate { get; set; }

    // Shared fields
    public string Overview { get; set; } = default!;

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }

    public List<TmdbGenre> Genres { get; set; } = [];
}

public class TmdbGenre
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}