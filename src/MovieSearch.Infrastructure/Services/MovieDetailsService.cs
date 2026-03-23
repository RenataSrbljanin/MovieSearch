using Microsoft.Extensions.Caching.Memory;
using MovieSearch.Application.Exceptions;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Models;
using MovieSearch.Infrastructure.Tmdb;

namespace MovieSearch.Infrastructure.Services;

public class MovieDetailsService : IMovieDetailsService
{
    private readonly TmdbClient _client;
    private readonly IMemoryCache _cache;

    public MovieDetailsService(TmdbClient client, IMemoryCache cache)
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

    // Univerzalna metoda koja hendluje i filmove i serije
    private async Task<MovieDetailsDto> GetWithCachingAsync(
        string id,
        string language,
        string type,
        Func<string, string, CancellationToken, Task<TmdbDetailsResponse>> detailsFunc,
        Func<string, string, CancellationToken, Task<TmdbVideosResponse>> videosFunc,
        CancellationToken ct)
    {
        if (!int.TryParse(id, out _))
            throw new BadRequestException($"Invalid {type} ID format.");

        var cacheKey = $"details:{type}:{id}:{language}";

        if (_cache.TryGetValue(cacheKey, out MovieDetailsDto? cached))
            return cached!;

        try
        {
            // Paralelno pozivanje oba endpointa za bolje performanse i skalabilnost
            var detailsTask = detailsFunc(id, language, ct);
            var videosTask = videosFunc(id, language, ct);

            await Task.WhenAll(detailsTask, videosTask);

            var result = MapDetails(detailsTask.Result, videosTask.Result, type);

            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            });

            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
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