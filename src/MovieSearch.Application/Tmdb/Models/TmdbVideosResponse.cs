using System.Text.Json.Serialization;

namespace MovieSearch.Application.Tmdb.Models;

public class TmdbVideosResponse
{
    public int Id { get; set; }
    public List<TmdbVideo> Results { get; set; } = [];
}

public class TmdbVideo
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Site { get; set; } = default!; // YouTube, Vimeo
    public string Type { get; set; } = default!; // Trailer, Teaser, Clip

    [JsonPropertyName("key")]
    public string Key { get; set; } = default!;
}