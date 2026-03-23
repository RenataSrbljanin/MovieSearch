using MovieSearch.Application.Models;

namespace MovieSearch.Application.Interfaces;

public interface IMovieDetailsService
{
    Task<MovieDetailsDto> GetMovieDetailsAsync(string id, string language, CancellationToken ct);
    Task<MovieDetailsDto> GetTvDetailsAsync(string id, string language, CancellationToken ct);
}