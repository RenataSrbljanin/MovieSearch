🎬 MovieSearch API (.NET 9)
A professional, production-ready .NET 9 Web API for searching movies and TV shows, integrated with The Movie Database (TMDb) API. This project demonstrates advanced software engineering practices, focusing on performance, security, and scalability.

🌟 Key Features & Architecture

  🏛️ Clean Architecture Implementation
  The project follows Clean Architecture principles to ensure a high degree of decoupling:

  Api Layer: Global Error Handling Middleware, Health Checks, and REST Controllers.

  Application Layer: Business logic, DTO mapping, and Service abstractions.

  Infrastructure Layer: Typed HTTP Clients and Redis configuration.

  🚀 Distributed Caching (Redis)
  Instead of basic in-memory caching, this API uses Redis (via Upstash).

  Horizontal Scalability: Multiple API instances share the same cache.

  Optimized Performance: Drastically reduces latency and TMDb API rate-limit consumption by storing frequent search results.

  🛡️ Resilience & Fault Tolerance (Polly)
  Integrated Microsoft.Extensions.Http.Resilience to handle transient failures:

  Standard Resilience Handler: Includes Retry, Circuit Breaker, and Rate Limiting to ensure the app stays alive even when external services "stutter".

  🧪 Robust Testing Suite
  The core business logic is fully protected by Unit Tests:

  Frameworks: xUnit & Moq.

  Coverage: Validates caching logic, API fallback mechanisms, and data transformation.

  🩺 Monitoring & Health Checks
  Implemented Health Checks for real-time monitoring:

  /health endpoint provides status reports for both the API and the Redis connection.

  📜 Secure Error Handling & Logging
  Global Middleware: Captures all exceptions, returning standardized JSON responses.

  Security-First Logging: Uses Serilog to log full stack traces internally while keeping the public API responses clean and safe from information leakage.

  🐳 Containerization (Docker)
  The application is fully containerized using a multi-stage Dockerfile.

  Optimized Build: Uses the SDK image for compiling and the lightweight ASP.NET runtime image for production, ensuring a small and fast container footprint.

  Port Configuration: Pre-configured to run on port 8080.

🛠️ Tech Stack

  Framework: .NET 9 (ASP.NET Core)

  Caching: Redis (Upstash)

  Resilience: Polly

  Logging: Serilog (File & Console)

  Testing: xUnit, Moq

  API Documentation: Swagger / OpenAPI

  Containerization: Docker

🚀 Getting Started

Prerequisites

  .NET 9 SDK

  TMDb API Key

  Upstash Redis account (for caching)

  Security & Configuration
  This project uses User Secrets to prevent sensitive data from being committed to version control.

Initialize User Secrets:
  Bash
  dotnet user-secrets init --project src/MovieSearch.Api

Set your credentials:
  Bash
  dotnet user-secrets set "Tmdb:ApiToken" "YOUR_TMDB_TOKEN" --project src/MovieSearch.Api
  dotnet user-secrets set "ConnectionStrings:RedisConnection" "YOUR_REDIS_URL" --project src/MovieSearch.Api

Installation & Run

Clone the repository.

Run the application:
  Bash
  dotnet run --project src/MovieSearch.Api

Execute tests:
  Bash
  dotnet test

Running with Docker:

    Build the image:

    Bash
    docker build -t moviesearch-api .
    Run the container (make sure to pass your secrets as environment variables):

    Bash
    docker run -p 8080:8080 \
      -e "Tmdb__ApiToken=YOUR_TOKEN" \
      -e "ConnectionStrings__RedisConnection=YOUR_REDIS_URL" \
      moviesearch-api

📋 Roadmap
[ ] CI/CD Pipeline: GitHub Actions for automated testing and deployment.
[ ] Kubernetes Manifests: Adding K8s YAML files for orchestration.
[ ] Authentication: Implementing JWT-based security for protected endpoints.
[ ] Monitoring Dashboard: Integrating Prometheus/Grafana for health metrics.