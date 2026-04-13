using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MovieSearch.Application.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MovieSearch.Application.Services;

public class IdentityService : IIdentityService
{
    private readonly IConfiguration _config;

    public IdentityService(IConfiguration config)
    {
        _config = config;
    }

    public string? Authenticate(LoginRequestDto request)
    {
        // 1. Provera kredencijala (Hardkodovano za demonstraciju)
        if (request.Username != "admin" || request.Password != "admin123")
            return null;

        // 2. Generisanje tokena
        return GenerateJwtToken(request.Username);
    }

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}