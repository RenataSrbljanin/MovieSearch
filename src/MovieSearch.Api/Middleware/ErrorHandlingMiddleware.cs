using System.Net;
using System.Text.Json;
using MovieSearch.Application.Exceptions;

namespace MovieSearch.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // loguje i Exception (sa celim Stack Trace-om) i lepu poruku sa putanjom za developera
            _logger.LogError(ex, "An unexpected error occurred during the request to {Path}. Error: {Message}",
            context.Request.Path,
            ex.Message);
            // ovde pozivam metodu koja filtrira odgovor za korisnika
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError; // Default 500

        // Mapiranje mojih custom izuzetaka na HTTP kodove
        if (exception is NotFoundException) code = HttpStatusCode.NotFound;
        else if (exception is BadRequestException) code = HttpStatusCode.BadRequest;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var errorMessage = exception.Message ?? "An unexpected error occurred.";

        var result = JsonSerializer.Serialize(new
        {
            error = errorMessage,
            statusCode = (int)code
            // StackTrace ide u logove (preko _logger-a), a NIKADA ovde ka korisniku!
        });
        return context.Response.WriteAsync(result);
    }
}