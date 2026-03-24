using System.Net;
using System.Text.Json;
using MovieSearch.Application.Exceptions;
using Serilog;

namespace MovieSearch.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred during the request: {Path}.");
            await HandleExceptionAsync(context, ex);
        }
    }
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = "An unexpected error occurred.";

        if (exception is NotFoundException) code = HttpStatusCode.NotFound;
        else if (exception is BadRequestException) code = HttpStatusCode.BadRequest;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var response = new { error = exception.Message ?? result };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}