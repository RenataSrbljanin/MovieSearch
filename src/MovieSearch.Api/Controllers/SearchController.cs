using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Models;

namespace MovieSearch.Api.Controllers;

[Authorize]
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
    [ProducesResponseType(typeof(MovieSearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieSearchResultDto>> Search(
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