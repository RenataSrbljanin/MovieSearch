using Microsoft.AspNetCore.Mvc;
using MovieSearch.Application.Interfaces;
using Asp.Versioning;

namespace MovieSearch.Api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly ICacheService _cache;
    private readonly ILogger<WebhooksController> _logger;
    private readonly IConfiguration _configuration;

    public WebhooksController(ICacheService cache, ILogger<WebhooksController> logger, IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Ručno čišćenje keša za specifičan film ili seriju (npr. nakon TMDb ažuriranja)
    /// </summary>
    [HttpPost("cache-invalidate/{type}/{id}/{language}")]
    public async Task<IActionResult> InvalidateCache(string type, string id, string language = "en-US")
    {
        // 1. Provera API ključa iz Header-a
    if (!Request.Headers.TryGetValue("X-Api-Key", out var extractedKey))
    {
        return Unauthorized("API Key is missing.");
    }

    var secureKey = _configuration["WebhookOptions:ApiKey"];

    if (secureKey != extractedKey)
    {
        return Unauthorized("Invalid API Key.");
    }
        //2. Ako je ključ ispravan, brišemo kešKljuč mora biti identičan šablonu koji koristi MovieDetailsService
        var cacheKey = $"details:{type.ToLower()}:{id}:{language}";
        
        _logger.LogInformation("Webhook received: Invalidating cache for key {CacheKey}", cacheKey);
        
        await _cache.RemoveAsync(cacheKey);
        
        return NoContent(); // 204 status je standard za uspešne webhook operacije
    }
}