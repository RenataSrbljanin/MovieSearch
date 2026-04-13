using Microsoft.Extensions.Configuration;
using Moq;
using MovieSearch.Application.Dtos;
using MovieSearch.Application.Services;
using Xunit;

namespace MovieSearch.Tests.Services;

public class IdentityServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        _configMock = new Mock<IConfiguration>();

        // Podešavamo lažnu konfiguraciju (mora da se poklapa sa onim što servis traži)
        _configMock.Setup(x => x["Jwt:Key"]).Returns("SuperSecretKey12345678901234567890");
        _configMock.Setup(x => x["Jwt:Issuer"]).Returns("MovieSearchApi");
        _configMock.Setup(x => x["Jwt:Audience"]).Returns("MovieSearchUsers");

        _service = new IdentityService(_configMock.Object);
    }

    [Fact]
    public void Authenticate_ValidCredentials_ReturnsToken()
    {
        // ARRANGE
        var request = new LoginRequestDto
        { 
            Username = "admin", 
            Password = "admin123" 
        };

        // ACT
        var result = _service.Authenticate(request);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Authenticate_InvalidCredentials_ReturnsNull()
    {
        // ARRANGE
        var request = new LoginRequestDto
        { 
            Username = "wrong", 
            Password = "password" 
        };

        // ACT
        var result = _service.Authenticate(request);

        // ASSERT
        Assert.Null(result);
    }
}