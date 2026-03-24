using Serilog;
using MovieSearch.Infrastructure.Tmdb;
using MovieSearch.Application.Interfaces;
using MovieSearch.Infrastructure.Services;
using MovieSearch.Api.Middleware;

// 1. Konfiguracija Seriloga pre svega ostalog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Movie Search API...");

    var builder = WebApplication.CreateBuilder(args);

    // 2. kazem aplikaciji da koristi Serilog umesto ugrađenog loggera
    builder.Host.UseSerilog();

    // 3. Konfiguracija i Infrastruktura
    builder.Services.Configure<TmdbOptions>(builder.Configuration.GetSection("Tmdb"));
    builder.Services.AddMemoryCache();

    // 4. HttpClient za TMDb - Typed Client (Rešava Socket Exhaustion)
    builder.Services.AddHttpClient<TmdbClient>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(2)); // Reuse HttpMessageHandler do 2 minuta

    // 5. Registracija servisa
    builder.Services.AddScoped<IMovieSearchService, MovieSearchService>();
    builder.Services.AddScoped<IMovieDetailsService, MovieDetailsService>();

    // 6. API Controllers
    builder.Services.AddControllers();

    // 7. OpenAPI / Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Movie Search API - Renata Srbljanin",
            Version = "v1",
            Description = "An API for searching movie trailers, built for DEPT® assessment."
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

        // Provera da li fajl postoji pre nego što ga Swagger učita (da ne pukne aplikacija)
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    var app = builder.Build();

    // 8. Middleware Pipeline (Redosled je bitan!)
    app.UseMiddleware<ErrorHandlingMiddleware>(); // Prvi, da uhvati sve greške ispod

    // Swagger samo u developmentu
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start correctly");
}
finally
{
    Log.CloseAndFlush();
}