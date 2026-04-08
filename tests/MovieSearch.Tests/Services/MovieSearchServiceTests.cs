using Moq;
using Xunit;
using Microsoft.Extensions.Caching.Distributed;
using MovieSearch.Application.Services;
using MovieSearch.Application.Interfaces;
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
        // "Mockujem" zavisnosti - pravim njihove lažne verzije
        _tmdbClientMock = new Mock<ITmdbClient>();
        _cacheMock = new Mock<IDistributedCache>();

        // Kreiram servis koji testiram, ubacujući mu te lažne objekte
        _service = new MovieSearchService(_tmdbClientMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnFromCache_WhenDataExists()
    {
        // ARRANGE (Priprema)
        var query = "Batman";
        var cacheKey = $"search:en:all:{query}:1";
        var cachedResponse = new MovieSearchResultDto
        {
            TotalResults = 1,
            Page = 1,
            TotalPages = 1,
            Results = new List<MovieSummaryDto>
            {
                new MovieSummaryDto
                {
                    Id = "123",
                    Title = "Batman Begins"
                }
            }
        };
        var serializedData = JsonSerializer.Serialize(cachedResponse);

        // Podešavam lažni keš da vrati podatke čim ga neko pita za taj ključ
        _cacheMock.Setup(x => x.GetAsync(cacheKey, default))
                  .ReturnsAsync(Encoding.UTF8.GetBytes(serializedData));

        // ACT (Izvršavanje)
        var result = await _service.SearchAsync(query, 1, "all", "en", CancellationToken.None);

        // ASSERT (Provera)
        Assert.NotNull(result);
        // Proveravam da se TMDb API NIJE pozvao, jer sam podatke imala u kešu
        _tmdbClientMock.Verify(x => x.SearchMultiAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task SearchAsync_ShouldCallTmdb_WhenCacheIsEmpty()
    {
        // ARRANGE
        var query = "Superman";
        var cacheKey = $"search:en:all:{query}:1";

        // Podešavam da lažni keš vrati NULL (prazno)
        _cacheMock.Setup(x => x.GetAsync(cacheKey, default))
                  .ReturnsAsync((byte[]?)null);

        // Pripremam lažni odgovor od TMDb klijenta
        var tmdbResponse = new TmdbSearchResponse
        {
            Page = 1,
            Results = new List<TmdbSearchItem>()
        };

        _tmdbClientMock.Setup(x => x.SearchMultiAsync(query, 1, "en", It.IsAny<CancellationToken>()))
                       .ReturnsAsync(tmdbResponse);

        // ACT
        await _service.SearchAsync(query, 1, "all", "en", CancellationToken.None);

        // ASSERT
        // Proveravam da li je TMDb klijent pozvan TAČNO JEDNOM
        _tmdbClientMock.Verify(x => x.SearchMultiAsync(query, 1, "en", It.IsAny<CancellationToken>()), Times.Once);

        // Proveravam da li je rezultat pokušao da se SPIŠE u keš nakon što je stigao sa API-ja
        _cacheMock.Verify(x => x.SetAsync(
            cacheKey,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}