using MovieSearch.Application.Tmdb.Models;

public interface ITmdbClient
{
    Task<TmdbSearchResponse> SearchMultiAsync(string query, int page, string language, CancellationToken ct);
    Task<TmdbDetailsResponse> GetMovieDetailsAsync(string id, string language, CancellationToken ct);
    Task<TmdbVideosResponse> GetMovieVideosAsync(string id, string language, CancellationToken ct);
    Task<TmdbDetailsResponse> GetTvDetailsAsync(string id, string language, CancellationToken ct);
    Task<TmdbVideosResponse> GetTvVideosAsync(string id, string language, CancellationToken ct);
    Task<TmdbSearchResponse> SearchMoviesAsync(string query, int page, string language, CancellationToken ct);
    Task<TmdbSearchResponse> SearchTvAsync(string query, int page, string language, CancellationToken ct);
}