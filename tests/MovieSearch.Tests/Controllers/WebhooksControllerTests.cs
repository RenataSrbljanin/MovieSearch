using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MovieSearch.Api.Controllers;
using MovieSearch.Application.Interfaces;

namespace MovieSearch.Tests.Controllers;

public class WebhooksControllerTests
{
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<WebhooksController>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly WebhooksController _controller;
    private const string ValidApiKey = "test-api-key-123";

    public WebhooksControllerTests()
    {
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<WebhooksController>>();

        // Kreiram realnu In-Memory konfiguraciju umesto Mock-a jer je lakša za rad
        var inMemorySettings = new Dictionary<string, string> {
            {"WebhookOptions:ApiKey", ValidApiKey}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _controller = new WebhooksController(_cacheMock.Object, _loggerMock.Object, _configuration);
        
        // Moram inicijalizovati ControllerContext da bih mogla da simuliram Headere
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task InvalidateCache_ValidKey_ReturnsNoContentAndRemovesCache()
    {
        // ARRANGE
        _controller.Request.Headers["X-Api-Key"] = ValidApiKey;
        var type = "movie";
        var id = "500";
        var lang = "en-US";

        // ACT
        var result = await _controller.InvalidateCache(type, id, lang);

        // ASSERT
        Assert.IsType<NoContentResult>(result);
        _cacheMock.Verify(x => x.RemoveAsync($"details:{type}:{id}:{lang}", default), Times.Once);
    }

    [Fact]
    public async Task InvalidateCache_InvalidKey_ReturnsUnauthorized()
    {
        // ARRANGE
        _controller.Request.Headers["X-Api-Key"] = "wrong-key";

        // ACT
        var result = await _controller.InvalidateCache("movie", "500", "en-US");

        // ASSERT
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid API Key.", unauthorizedResult.Value);
        _cacheMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task InvalidateCache_MissingKey_ReturnsUnauthorized()
    {
        // ARRANGE - Request.Headers je prazan po defaultu

        // ACT
        var result = await _controller.InvalidateCache("movie", "500", "en-US");

        // ASSERT
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("API Key is missing.", unauthorizedResult.Value);
    }
}