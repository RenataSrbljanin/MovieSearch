using Microsoft.Extensions.Options;
using Moq;
using MovieSearch.Application.Common;
using MovieSearch.Application.Dtos;
using MovieSearch.Application.Interfaces; // Dodato
using MovieSearch.Application.Services;
using System.IdentityModel.Tokens.Jwt;

namespace MovieSearch.Tests.Services;

public class IdentityServiceTests
{
    private readonly Mock<ICacheService> _cacheMock; // Promenjeno na ICacheService
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        _cacheMock = new Mock<ICacheService>(); // Promenjeno
        
        var options = new JwtOptions
        {
            Key = "SuperSecretKey12345678901234567890",
            Issuer = "MovieSearchApi",
            Audience = "MovieSearchUsers",
            TokenExpirationInMinutes = 15,
            RefreshTokenExpirationInDays = 7
        };
        _jwtOptions = Options.Create(options);

        _service = new IdentityService(_jwtOptions, _cacheMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsAuthResponseAndSavesToCache()
    {
        // ARRANGE
        var request = new LoginRequestDto { Username = "admin", Password = "admin123" };

        // ACT
        var result = await _service.AuthenticateAsync(request);

        // ASSERT
        Assert.NotNull(result);
        
        // Mnogo jednostavnija provera za ICacheService
        _cacheMock.Verify(x => x.SetAsync(
            It.Is<string>(s => s.StartsWith("refreshToken:")),
            "admin", // Proveravamo da li šalje username kao string
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), 
        Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidCredentials_ReturnsNull()
    {
        // ARRANGE
        var request = new LoginRequestDto { Username = "hacker", Password = "wrongpassword" };

        // ACT
        var result = await _service.AuthenticateAsync(request);

        // ASSERT
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ContainsCorrectClaims()
    {
        // ARRANGE
        var username = "admin";
        var request = new LoginRequestDto { Username = username, Password = "admin123" };

        // ACT
        var result = await _service.AuthenticateAsync(request);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result!.AccessToken);

        // ASSERT
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.NotNull(subClaim);
        Assert.Equal(username, subClaim.Value);
        Assert.Equal(_jwtOptions.Value.Issuer, jwtToken.Issuer);
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
    }
  
    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokensAndRemovesOldOne()
    {
        // ARRANGE
        var validRefreshToken = "valid-token-123";
        var username = "admin";

        // Setup je sada lakši jer ICacheService radi sa <T> (u ovom slučaju string)
        _cacheMock.Setup(x => x.GetAsync<string>($"refreshToken:{validRefreshToken}", default))
                  .ReturnsAsync(username);

        var request = new RefreshRequestDto { RefreshToken = validRefreshToken };

        // ACT
        var result = await _service.RefreshTokenAsync(request);

        // ASSERT
        Assert.NotNull(result);
        _cacheMock.Verify(x => x.RemoveAsync($"refreshToken:{validRefreshToken}", default), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ReturnsNull()
    {
        // ARRANGE
        var invalidToken = "non-existent-token";
        
        // Vraćamo null kao string
        _cacheMock.Setup(x => x.GetAsync<string>($"refreshToken:{invalidToken}", default))
                  .ReturnsAsync((string?)null);

        var request = new RefreshRequestDto { RefreshToken = invalidToken };

        // ACT
        var result = await _service.RefreshTokenAsync(request);

        // ASSERT
        Assert.Null(result);
    }
}