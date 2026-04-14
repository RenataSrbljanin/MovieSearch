using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using MovieSearch.Application.Common;
using MovieSearch.Application.Dtos;
using MovieSearch.Application.Services;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace MovieSearch.Tests.Services;

public class IdentityServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        
        // Podešavam JwtOptions direktno preko Options.Create 
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

    [Fact]  // uspesna prijava i spremanje u Redis
    public async Task AuthenticateAsync_ValidCredentials_ReturnsAuthResponseAndSavesToCache()
    {
        // ARRANGE
        var request = new LoginRequestDto
        { 
            Username = "admin", 
            Password = "admin123" 
        };

        // ACT
        var result = await _service.AuthenticateAsync(request);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        
        // Proveravam da li je servis pokušao da sačuva refresh token u Redis
        _cacheMock.Verify(x => x.SetAsync(
            It.Is<string>(s => s.StartsWith("refreshToken:")),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), 
        Times.Once);
    }

    [Fact]  // neuspesna prijava
    public async Task AuthenticateAsync_InvalidCredentials_ReturnsNull()
    {
        // ARRANGE
        var request = new LoginRequestDto
        { 
            Username = "hacker", 
            Password = "wrongpassword" 
        };

        // ACT
        var result = await _service.AuthenticateAsync(request);

        // ASSERT
        Assert.Null(result);
    }

    [Fact]  // ispravnost samog tokena
    public async Task AuthenticateAsync_ValidCredentials_ContainsCorrectClaims()
    {
        // ARRANGE
        var username = "admin";
        var request = new LoginRequestDto { Username = username, Password = "admin123" };

        // ACT
        var result = await _service.AuthenticateAsync(request);
        
        // DEKODIRANJE TOKENA
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result!.AccessToken);

        // ASSERT
        // Proveravam da li 'sub' claim odgovara korisničkom imenu
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.NotNull(subClaim);
        Assert.Equal(username, subClaim.Value);

        // Proveravam da li je Issuer ispravno postavljen iz mojih opcija
        Assert.Equal(_jwtOptions.Value.Issuer, jwtToken.Issuer);
        
        // Proveravam da li token ima JTI (Unique ID), što je bitno za sprečavanje replay napada
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
    }
  
    [Fact]  // uspesan Refresh uz brisanje starog tokena
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokensAndRemovesOldOne()
    {
        // ARRANGE
        var validRefreshToken = "valid-token-123";
        var username = "admin";
        var usernameBytes = Encoding.UTF8.GetBytes(username);

        // Simuliram da Redis pronađe korisnika za taj token
        _cacheMock.Setup(x => x.GetAsync($"refreshToken:{validRefreshToken}", default))
                  .ReturnsAsync(usernameBytes);

        var request = new RefreshRequestDto { RefreshToken = validRefreshToken };

        // ACT
        var result = await _service.RefreshTokenAsync(request);

        // ASSERT
        Assert.NotNull(result);
        // Proveravam da li je stari token obrisan iz keša (One-time use politika)
        _cacheMock.Verify(x => x.RemoveAsync($"refreshToken:{validRefreshToken}", default), Times.Once);
    }

    [Fact]  // neuspesan Refresh - prazan ili krivi token
    public async Task RefreshTokenAsync_InvalidToken_ReturnsNull()
    {
        // ARRANGE
        var invalidToken = "non-existent-token";
        // Simuliram da Redis ne pronađe ništa (vraća null)
        _cacheMock.Setup(x => x.GetAsync($"refreshToken:{invalidToken}", default))
                  .ReturnsAsync((byte[]?)null);

        var request = new RefreshRequestDto { RefreshToken = invalidToken };

        // ACT
        var result = await _service.RefreshTokenAsync(request);

        // ASSERT
        Assert.Null(result);
    }

    [Fact]  // neuspesan Refresh - simulacija da je token postojao ali je istekao u Redis-u
    public async Task RefreshTokenAsync_ExpiredTokenInRedis_ReturnsNull()
    {
        // ARRANGE
        var expiredToken = "expired-token-456";
        // Redis vraća null jer je ključ istekao
        _cacheMock.Setup(x => x.GetAsync($"refreshToken:{expiredToken}", default))
                  .ReturnsAsync((byte[]?)null);

        var request = new RefreshRequestDto { RefreshToken = expiredToken };

        // ACT
        var result = await _service.RefreshTokenAsync(request);

        // ASSERT
        Assert.Null(result);
    }
  }