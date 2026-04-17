using System.Text.Json;
using MovieSearch.Application.Exceptions;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Models;
using MovieSearch.Application.Tmdb.Models;

namespace MovieSearch.Application.Services;

public class MovieDetailsService : IMovieDetailsService
{
    private readonly ITmdbClient _client;
    private readonly ICacheService _cache;

    public MovieDetailsService(ITmdbClient client, ICacheService cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<MovieDetailsDto> GetMovieDetailsAsync(string id, string language, CancellationToken ct)
    {
        return await GetWithCachingAsync(
                    id,
                    language,
                    "movie",
                    _client.GetMovieDetailsAsync,
                    _client.GetMovieVideosAsync,
                    ct);
    }

    public async Task<MovieDetailsDto> GetTvDetailsAsync(string id, string language, CancellationToken ct)
    {
        return await GetWithCachingAsync(
                    id,
                    language,
                    "tv",
                    _client.GetTvDetailsAsync,
                    _client.GetTvVideosAsync,
                    ct);
    }

    // Univerzalna metoda koja hendluje i filmove i serije uz korišćenje apstrahovanog keš servisa
    private async Task<MovieDetailsDto> GetWithCachingAsync(
        string id,
        string language,
        string type,
        Func<string, string, CancellationToken, Task<TmdbDetailsResponse>> detailsFunc,
        Func<string, string, CancellationToken, Task<TmdbVideosResponse>> videosFunc,
        CancellationToken ct)
    {
        // 1. Validacija ID formata (Security & Integrity check)
        if (!int.TryParse(id, out _))
            throw new BadRequestException($"Invalid {type} ID format.");

        var cacheKey = $"details:{type}:{id}:{language}";

        // 2. Pokušaj dobavljanja iz keša. ICacheService sada interno hendluje 
        // asinhronost i deserijalizaciju iz JSON-a u MovieDetailsDto.
        var cachedResult = await _cache.GetAsync<MovieDetailsDto>(cacheKey, ct);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        try
        {
            // 3. Paralelno pozivanje TMDb API endpointa (Details + Videos)
            // Koristim Task.WhenAll da bih smanjila ukupno vreme čekanja (Latency)
            var detailsTask = detailsFunc(id, language, ct);
            var videosTask = videosFunc(id, language, ct);

            await Task.WhenAll(detailsTask, videosTask);

            // 4. Mapiranje TMDb modela u moj DTO
            var result = MapDetails(detailsTask.Result, videosTask.Result, type);

            // 5. Upisivanje u Redis. Logika o trajanju keša (TTL) je enkapsulirana u servisu,
            // što ovaj kod čini fokusiranim isključivo na tok podataka.
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), ct);

            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // 6. Specifično hvatanje 404 greške i transformacija u domen-specifičnu NotFoundException
            throw new NotFoundException($"{type} with ID {id} not found.");
        }
    }
    private MovieDetailsDto MapDetails(TmdbDetailsResponse details, TmdbVideosResponse videos, string type)
    {
        return new MovieDetailsDto
        {
            Id = details.Id.ToString(),
            Type = type,
            Title = type == "movie" ? details.Title ?? "Unknown title" : details.Name ?? "Unknown title",
            Overview = details.Overview,
            PosterUrl = details.PosterPath is null ? null : $"https://image.tmdb.org/t/p/w500{details.PosterPath}",
            BackdropUrl = details.BackdropPath is null ? null : $"https://image.tmdb.org/t/p/w780{details.BackdropPath}",
            ReleaseDate = type == "movie" ? details.ReleaseDate : details.FirstAirDate,
            Rating = details.VoteAverage,
            Genres = details.Genres.Select(g => g.Name),
            Trailers = videos.Results
                .Where(v => v.Type == "Trailer" && v.Site == "YouTube")
                .Select(v => new TrailerDto
                {
                    Provider = "YouTube",
                    Name = v.Name,
                    Url = $"https://www.youtube.com/watch?v={v.Key}"
                })
        };
    }
}