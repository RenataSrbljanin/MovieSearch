using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Models;

namespace MovieSearch.Api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/details")]
public class MovieDetailsController : ControllerBase
{
    private readonly IMovieDetailsService _service;

    public MovieDetailsController(IMovieDetailsService service)
    {
        _service = service;
    }

    [HttpGet("movie/{id}")]
    [ProducesResponseType(typeof(MovieDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailsDto>> GetMovie(string id, string language = "en-US", CancellationToken ct = default)
    {
        var result = await _service.GetMovieDetailsAsync(id, language, ct);
        return Ok(result);
    }

    [HttpGet("tv/{id}")]
    [ProducesResponseType(typeof(MovieDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailsDto>> GetTv(string id, string language = "en-US", CancellationToken ct = default)
    {
        var result = await _service.GetTvDetailsAsync(id, language, ct);
        return Ok(result);
    }
}