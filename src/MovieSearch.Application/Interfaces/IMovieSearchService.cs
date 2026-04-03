using MovieSearch.Application.Models;

namespace MovieSearch.Application.Interfaces;

public interface IMovieSearchService
{
    Task<MovieSearchResultDto> SearchAsync(
        string query,
        int page,
        string type,
        string language,
        CancellationToken cancellationToken);
}