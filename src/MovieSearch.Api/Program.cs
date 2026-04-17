using Serilog;
using MovieSearch.Infrastructure.Tmdb;
using MovieSearch.Application.Interfaces;
using MovieSearch.Api.Middleware;
using System.Net.Http.Headers;
using Polly;
using MovieSearch.Application.Services;
// Dodatni using-ovi za JWT
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MovieSearch.Application.Common;
using Asp.Versioning;
using MovieSearch.Infrastructure.Caching;

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
    // Dodajem tipiziranu konfiguraciju za JWT opcije
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
    // Redis Cache - umesto lokalnog memorijskog cache-a, koristim Redis koji je eksterni servis, skalabilan i deljen između više instanci aplikacije
    var redisConn = builder.Configuration.GetConnectionString("RedisConnection");

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = "MovieSearch_";
    });

    // 4. HttpClient za TMDb - Typed Client (Rešava Socket Exhaustion)
    // Izvlačim opcije ranije da bih ih koristila u konfiguraciji klijenta
    var tmdbOptions = builder.Configuration.GetSection("Tmdb").Get<TmdbOptions>()
        ?? throw new InvalidOperationException("TMDB configuration is missing in appsettings.json!");

    builder.Services.AddHttpClient<ITmdbClient, TmdbClient>(client =>
    {
        // Konfiguracija klijenta se dešava OVDE, a ne u konstruktoru klase
        client.BaseAddress = new Uri(tmdbOptions.BaseUrl);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tmdbOptions.ApiToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(2))
    .AddStandardResilienceHandler(options =>  // OVDE DODAJEM POLLY!
{
    // Ovde fino podesavam pravila:

    options.Retry.MaxRetryAttempts = 3;  // 1. Retry: Ako ne uspe, probaj opet 3 puta
    options.Retry.Delay = TimeSpan.FromSeconds(2); // Sačekaj 2s pre ponovnog pokušaja
    options.Retry.BackoffType = DelayBackoffType.Exponential; // Svaki sledeći put čekaj duže (2s, 4s, 8s)

    // 2. Circuit Breaker: Ako TMDB konstantno greši, "isključi osigurač" na neko vreme (30sec)
    // da ne mučim server koji je očigledno "mrtav"-ovo cuva resurse mog servera
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
});

    // 5. Registracija servisa
    builder.Services.AddScoped<IIdentityService, IdentityService>();
    builder.Services.AddScoped<IMovieSearchService, MovieSearchService>();
    builder.Services.AddScoped<IMovieDetailsService, MovieDetailsService>();
    builder.Services.AddScoped<ICacheService, RedisCacheService>();

    // 6. API Controllers
    builder.Services.AddControllers();

    // 7. Registracija Health Check-ova
    builder.Services.AddHealthChecks()
        // Dodajem proveru za Redis koristeći moj Connection String
        .AddRedis(
            builder.Configuration.GetConnectionString("RedisConnection")!,
            name: "redis_check",
            tags: new[] { "ready" });

    // 8. OpenAPI / Swagger
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
        // Uvodim JWT autentifikaciju u Swagger da bih mogla testirati zaštićene endpoint-e direktno iz UI-a
        // 1. Definišem kako će Swagger tretirati JWT token
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Unesite samo JWT token (bez 'Bearer' prefiksa)."
        });
        // 1.1. Dodajem i API Key definiciju za Webhook endpoint
        options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "Unesite vaš API ključ za Webhook zaštitu",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Name = "X-Api-Key",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "ApiKeyScheme"
        });
        // 2. Kažem Swagger-u da primeni tu definiciju na sve endpointe
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            },
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
        });

        // govori Swaggeru kako da tretira rute koje imaju {version}
        options.DocInclusionPredicate((version, desc) =>
        {
            return true;
        });
    });

    // 9. JWT - Authentication & Authorization
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            // Postavljam ClockSkew na nulu kako bi istek tokena bio trenutan i precizan
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    var app = builder.Build();

    // 10. Middleware Pipeline (Redosled je bitan!)
    app.UseMiddleware<ErrorHandlingMiddleware>(); // Prvi, da uhvati sve greške ispod

    // 11. Swagger samo u developmentu
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication(); // Pre Authorization!
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");  // Mapiranje rute za Health Check

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