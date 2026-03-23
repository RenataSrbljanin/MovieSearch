using Microsoft.AspNetCore.Mvc;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Models;

namespace MovieSearch.Api.Controllers;

[ApiController]
[Route("api/details")]
public class MovieDetailsController : ControllerBase
{
    private readonly IMovieDetailsService _service;

    public MovieDetailsController(IMovieDetailsService service)
    {
        _service = service;
    }

    [HttpGet("movie/{id}")]
    [ProducesResponseType(typeof(MovieDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailsDto>> GetMovie(string id, string language = "en-US", CancellationToken ct = default)
    {
        var result = await _service.GetMovieDetailsAsync(id, language, ct);
        return Ok(result);
    }

    [HttpGet("tv/{id}")]
    [ProducesResponseType(typeof(MovieDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailsDto>> GetTv(string id, string language = "en-US", CancellationToken ct = default)
    {
        var result = await _service.GetTvDetailsAsync(id, language, ct);
        return Ok(result);
    }
}