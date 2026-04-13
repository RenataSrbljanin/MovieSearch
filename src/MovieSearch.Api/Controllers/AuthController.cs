using Microsoft.AspNetCore.Mvc;
using MovieSearch.Application.Dtos;

namespace MovieSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        var token = _identityService.Authenticate(request);
        if (token == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }
        return Ok(new { Token = token });
    }
}