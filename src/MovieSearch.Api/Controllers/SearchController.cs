using Microsoft.AspNetCore.Mvc;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Models;

namespace MovieSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMovieSearchService _searchService;

    public SearchController(IMovieSearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Searches for movies and TV shows based on the query.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(MovieSearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieSearchResult>> Search(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] string type = "all",
        [FromQuery] string language = "en-US",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query is required.");
        if (page < 1)
            return BadRequest(new { Message = "Page number must be greater than 0." });
            
        var result = await _searchService.SearchAsync(
            query, page, type, language, cancellationToken);

        return Ok(result);
    }
}