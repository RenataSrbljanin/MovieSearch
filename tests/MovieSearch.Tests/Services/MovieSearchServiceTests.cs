using Moq;
using Microsoft.Extensions.Caching.Distributed;
using MovieSearch.Application.Services;
using MovieSearch.Application.Tmdb.Models;
using System.Text;
using System.Text.Json;
using MovieSearch.Application.Models;

namespace MovieSearch.Tests.Services;

public class MovieSearchServiceTests
{
    private readonly Mock<ITmdbClient> _tmdbClientMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly MovieSearchService _service;

    public MovieSearchServiceTests()
    {
        _tmdbClientMock = new Mock<ITmdbClient>();
        _cacheMock = new Mock<IDistributedCache>();
        _service = new MovieSearchService(_tmdbClientMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnFromCache_WhenDataExists()
    {
        // ARRANGE
        var query = "Batman";
        var cacheKey = $"search:en:all:{query}:1";
        var cachedResponse = new MovieSearchResultDto
        {
            TotalResults = 1,
            Results = new List<MovieSummaryDto> { new MovieSummaryDto { Id = "123", Title = "Batman Begins" } }
        };
        var serializedData = JsonSerializer.Serialize(cachedResponse);

        // Koristim GetStringAsync jer moj servis koristi tu ekstenziju
        _cacheMock.Setup(x => x.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Encoding.UTF8.GetBytes(serializedData));

        // ACT
        var result = await _service.SearchAsync(query, 1, "all", "en", CancellationToken.None);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal("Batman Begins", result.Results.First().Title);
        // Provera da TMDB NIJE pozvan
        _tmdbClientMock.Verify(x => x.SearchMultiAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ShouldCallSpecificMovieEndpoint_WhenTypeIsMovie()
    {
        // ARRANGE
        var query = "Inception";
        var type = "movie";
        var cacheKey = $"search:en:{type}:{query}:1";

        _cacheMock.Setup(x => x.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);

        _tmdbClientMock.Setup(x => x.SearchMoviesAsync(query, 1, "en", It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new TmdbSearchResponse { Results = new List<TmdbSearchItem>() });

        // ACT
        await _service.SearchAsync(query, 1, type, "en", CancellationToken.None);

        // ASSERT
        // Ključni dokaz: Proveravam da je pozvan SearchMoviesAsync, a NE SearchMultiAsync
        _tmdbClientMock.Verify(x => x.SearchMoviesAsync(query, 1, "en", It.IsAny<CancellationToken>()), Times.Once);
        _tmdbClientMock.Verify(x => x.SearchMultiAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ShouldCallTmdbMulti_WhenTypeIsAllAndCacheIsEmpty()
    {
        // ARRANGE
        var query = "Superman";
        var cacheKey = $"search:en:all:{query}:1";

        _cacheMock.Setup(x => x.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);

        _tmdbClientMock.Setup(x => x.SearchMultiAsync(query, 1, "en", It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new TmdbSearchResponse { Results = new List<TmdbSearchItem>() });

        // ACT
        await _service.SearchAsync(query, 1, "all", "en", CancellationToken.None);

        // ASSERT
        _tmdbClientMock.Verify(x => x.SearchMultiAsync(query, 1, "en", It.IsAny<CancellationToken>()), Times.Once);
        
        // Provera da li je upisano u keš
        _cacheMock.Verify(x => x.SetAsync(
            cacheKey,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}