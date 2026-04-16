using Microsoft.IdentityModel.Tokens;
using MovieSearch.Application.Dtos;
using MovieSearch.Application.Common;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MovieSearch.Application.Interfaces;

namespace MovieSearch.Application.Services;

public class IdentityService : IIdentityService
{
    private readonly JwtOptions _jwtOptions;
    private readonly ICacheService _cache;

    public IdentityService(IOptions<JwtOptions> jwtOptions, ICacheService cache)
    {
        _jwtOptions = jwtOptions.Value;
        _cache = cache;
    }

    public async Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request)
    {
        // 1. Provera kredencijala (Zadržavam admin/admin123 logiku)
        if (request.Username != "admin" || request.Password != "admin123")
            return null;

        // 2. Generisanje oba tokena
        return await GenerateAuthResponse(request.Username);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshRequestDto request)
    {
        var cacheKey = $"refreshToken:{request.RefreshToken}";
        // Pokušavam da pronađem korisničko ime povezano sa ovim refresh tokenom u Redisu
        var username = await _cache.GetAsync<string>(cacheKey);

        if (string.IsNullOrEmpty(username))
            return null;

        // Brišem stari token (one-time use politika radi veće bezbednosti)
        await _cache.RemoveAsync(cacheKey);

        // Generišem novi par tokena
        return await GenerateAuthResponse(username);
    }

    private async Task<AuthResponseDto> GenerateAuthResponse(string username)
    {
        var accessToken = GenerateJwtToken(username);
        var refreshToken = GenerateRandomToken();

        // Čuvam refresh token u Redis-u koristeći ICacheService
        // Trajanje (Expiration) prosleđujem direktno metodi
        await _cache.SetAsync(
            $"refreshToken:{refreshToken}",
            username,
            TimeSpan.FromDays(_jwtOptions.RefreshTokenExpirationInDays));

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtOptions.TokenExpirationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRandomToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}