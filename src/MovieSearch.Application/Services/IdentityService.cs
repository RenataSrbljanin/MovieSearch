using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using MovieSearch.Application.Dtos;
using MovieSearch.Application.Common;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MovieSearch.Application.Services;

public class IdentityService : IIdentityService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IDistributedCache _cache;

    public IdentityService(IOptions<JwtOptions> jwtOptions, IDistributedCache cache)
    {
        _jwtOptions = jwtOptions.Value;
        _cache = cache;
    }

    public async Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request)
    {
        // 1. Provera kredencijala (Zadržavam tvoju admin/admin123 logiku)
        if (request.Username != "admin" || request.Password != "admin123")
            return null;

        // 2. Generisanje oba tokena
        return await GenerateAuthResponse(request.Username);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshRequestDto request)
    {
        // Pokušavam da pronađem korisničko ime povezano sa ovim refresh tokenom u Redisu
        var username = await _cache.GetStringAsync($"refreshToken:{request.RefreshToken}");

        if (string.IsNullOrEmpty(username))
            return null;

        // Brišem stari token (one-time use politika radi veće bezbednosti)
        await _cache.RemoveAsync($"refreshToken:{request.RefreshToken}");

        // Generišem novi par tokena
        return await GenerateAuthResponse(username);
    }

    private async Task<AuthResponseDto> GenerateAuthResponse(string username)
    {
        var accessToken = GenerateJwtToken(username);
        var refreshToken = GenerateRandomToken();

        // Čuvam refresh token u Redis-u sa trajanjem koje sam definisala u opcijama
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_jwtOptions.RefreshTokenExpirationInDays)
        };

        await _cache.SetStringAsync($"refreshToken:{refreshToken}", username, cacheOptions);

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