using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Models;
using MovieSearch.Application.Tmdb.Models;

namespace MovieSearch.Application.Services;

public class MovieSearchService : IMovieSearchService
{
    private readonly ITmdbClient _tmdb;
    private readonly IDistributedCache _cache;


    public MovieSearchService(ITmdbClient tmdb, IDistributedCache cache)
    {
        _tmdb = tmdb;
        _cache = cache;
    }

    public async Task<MovieSearchResultDto> SearchAsync(
        string query,
        int page,
        string type,
        string language,
        CancellationToken cancellationToken)
    {
        // 1. Validacija
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty.");

        // 2. Cache logika - kreiram ključ na osnovu parametara
        var cacheKey = $"search:{language}:{type}:{query}:{page}";// Redosled od opšteg ka specifičnom

        // Try get from cache
        // Čitanje iz Redisa (asinhrono)
        var cachedJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            // Vraćamo objekat iz JSON-a
            return JsonSerializer.Deserialize<MovieSearchResultDto>(cachedJson)!;
        }

        // 3. If not in cache, call TMDb API
        var tmdbResponse = await _tmdb.SearchMultiAsync(query, page, language, cancellationToken);

        // 4. Mapiranje rezultata - filtriranje po tipu i mapiranje u DTO
        var results = tmdbResponse.Results
            .Where(r => type == "all" || r.Media_Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            .Select(MapToSummaryDto)
            .ToList();

        var result = new MovieSearchResultDto
        {
            Page = tmdbResponse.Page,
            TotalPages = tmdbResponse.Total_Pages,
            TotalResults = tmdbResponse.Total_Results,
            Results = results
        };
        // 5. Pisanje u Redis
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromSeconds(60)
        };
        var serializedResult = JsonSerializer.Serialize(result);
        await _cache.SetStringAsync(cacheKey, serializedResult, cacheOptions, cancellationToken);

        return result;

    }
    // Pomoćna metoda za čistiji kod
    private MovieSummaryDto MapToSummaryDto(TmdbSearchItem r) => new MovieSummaryDto
    {
        Id = r.Id.ToString(),
        Type = r.Media_Type,
        Title = r.Title ?? r.Name ?? "Unknown",
        Overview = r.Overview ?? "",
        PosterUrl = !string.IsNullOrEmpty(r.Poster_Path)
            ? $"https://image.tmdb.org/t/p/w500{r.Poster_Path}"
            : null,
        ReleaseDate = r.Release_Date ?? r.First_Air_Date,
        Rating = r.Vote_Average
    };
}