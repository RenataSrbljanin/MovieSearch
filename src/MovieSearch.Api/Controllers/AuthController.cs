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
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        // Pozivam asinhronu metodu koja sada vraća AuthResponseDto (AccessToken + RefreshToken)
        var authResponse = await _identityService.AuthenticateAsync(request);
        
        if (authResponse == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        return Ok(authResponse);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        // Metoda koja validira refresh token u Redis-u i generiše novi set tokena
        var authResponse = await _identityService.RefreshTokenAsync(request);
        
        if (authResponse == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        return Ok(authResponse);
    }
}