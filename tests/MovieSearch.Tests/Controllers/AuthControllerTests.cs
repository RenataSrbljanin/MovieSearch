using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieSearch.Api.Controllers;
using MovieSearch.Application.Dtos;

namespace MovieSearch.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _controller = new AuthController(_identityServiceMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithTokens()
    {
        // ARRANGE
        var request = new LoginRequestDto { Username = "admin", Password = "admin123" };
        var expectedResponse = new AuthResponseDto 
        { 
            AccessToken = "access-token", 
            RefreshToken = "refresh-token" 
        };

        // Postavljam mock da vrati uspešan odgovor
        _identityServiceMock
            .Setup(x => x.AuthenticateAsync(request))
            .ReturnsAsync(expectedResponse);

        // ACT
        var result = await _controller.Login(request);

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnData = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal(expectedResponse.AccessToken, returnData.AccessToken);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // ARRANGE
        var request = new LoginRequestDto { Username = "wrong", Password = "wrong" };
        
        // Postavljam mock da vrati null (neuspešna prijava)
        _identityServiceMock
            .Setup(x => x.AuthenticateAsync(request))
            .ReturnsAsync((AuthResponseDto?)null);

        // ACT
        var result = await _controller.Login(request);

        // ASSERT
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsOkWithNewTokens()
    {
        // ARRANGE
        var request = new RefreshRequestDto { RefreshToken = "valid-refresh-token" };
        var expectedResponse = new AuthResponseDto 
        { 
            AccessToken = "new-access-token", 
            RefreshToken = "new-refresh-token" 
        };

        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync(request))
            .ReturnsAsync(expectedResponse);

        // ACT
        var result = await _controller.Refresh(request);

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<AuthResponseDto>(okResult.Value);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        // ARRANGE
        var request = new RefreshRequestDto { RefreshToken = "invalid-token" };

        _identityServiceMock
            .Setup(x => x.RefreshTokenAsync(request))
            .ReturnsAsync((AuthResponseDto?)null);

        // ACT
        var result = await _controller.Refresh(request);

        // ASSERT
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}