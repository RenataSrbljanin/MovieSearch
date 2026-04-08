using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MovieSearch.Application.Tmdb.Models;
namespace MovieSearch.Infrastructure.Tmdb;

public class TmdbClient : ITmdbClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    public TmdbClient(HttpClient httpClient, IOptions<TmdbOptions> options)
    {
        _httpClient = httpClient;
        // TMDB koristi snake_case, pa ovo dodajem da ne bih morala ručno mapirati svako polje
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public async Task<TmdbSearchResponse> SearchMultiAsync(
        string query, int page, string language, CancellationToken ct)
    {
        var url = $"search/multi?query={Uri.EscapeDataString(query)}&page={page}&language={language}";
        return await GetAsync<TmdbSearchResponse>(url, ct);
    }
    // Pomoćna generička metoda - Štedi linije koda i koristi streamove za performanse
    private async Task<T> GetAsync<T>(string url, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        // ReadFromJsonAsync čita direktno iz stream-a
        var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, ct);

        return result ?? throw new InvalidOperationException("Failed to deserialize TMDB response.");
    }
    // ovde dodajem metode za dobijanje detalja o filmu i seriji
    public async Task<TmdbDetailsResponse> GetMovieDetailsAsync(string id, string language, CancellationToken ct)
    => await GetAsync<TmdbDetailsResponse>($"movie/{id}?language={language}", ct);


    public async Task<TmdbVideosResponse> GetMovieVideosAsync(string id, string language, CancellationToken ct)
    => await GetAsync<TmdbVideosResponse>($"movie/{id}/videos?language={language}", ct);


    public async Task<TmdbDetailsResponse> GetTvDetailsAsync(string id, string language, CancellationToken ct)
    => await GetAsync<TmdbDetailsResponse>($"tv/{id}?language={language}", ct);

    public async Task<TmdbVideosResponse> GetTvVideosAsync(string id, string language, CancellationToken ct)
    => await GetAsync<TmdbVideosResponse>($"tv/{id}/videos?language={language}", ct);

    public async Task<TmdbSearchResponse> SearchMoviesAsync(string query, int page, string language, CancellationToken cancellationToken)
    {
        // Koristim "search/movie" umesto "search/multi"
        return await GetAsync<TmdbSearchResponse>($"search/movie?query={Uri.EscapeDataString(query)}&page={page}&language={language}", cancellationToken);
    }

    public async Task<TmdbSearchResponse> SearchTvAsync(string query, int page, string language, CancellationToken cancellationToken)
    {
        // Koristim "search/tv" umesto "search/multi"
        return await GetAsync<TmdbSearchResponse>($"search/tv?query={Uri.EscapeDataString(query)}&page={page}&language={language}", cancellationToken);
    }
}
