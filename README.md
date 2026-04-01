# MovieSearch API (.NET 9)

A modern, scalable, and resilient .NET 9 Web API for searching movies and TV shows, integrated with [The Movie Database (TMDb) API](https://www.themoviedb.org/).

## 🌟 Key Features & Architecture Improvements

Following an architectural review, this project has been significantly refactored to meet production-grade standards:

### 🏛️ Clean Architecture Implementation
The project follows **Clean Architecture** principles to ensure a high degree of decoupling and testability:
- **Api Layer:** Handles HTTP requests, Middleware, and Controllers.
- **Application Layer:** Contains all business logic, Service interfaces, and DTOs.
- **Infrastructure Layer:** Handles external concerns like HTTP communication with TMDb and configuration.

### 🛡️ Resilience & Fault Tolerance (Polly)
Integrated `Microsoft.Extensions.Http.Resilience` to handle transient failures. 
- Implemented the **Standard Resilience Handler**, which automatically provides:
    - **Retry Policy:** Retries requests on temporary network glitches.
    - **Circuit Breaker:** Prevents overwhelming the external API during outages.
    - **Timeout & Rate Limiting:** Ensures the application remains responsive.

### 📡 Typed & Named HTTP Clients
Used the **Typed Client** pattern for `ITmdbClient`. This encapsulates all TMDb-specific logic (Base URL, Authorization Headers) and prevents "leaking" infrastructure details into the business layer.

### 📜 Structured Logging (Serilog)
Implemented **Serilog** for structured logging. This provides deep visibility into the request pipeline, performance metrics, and detailed error context, making it much easier to debug in production environments.

### 🔄 Data Transfer Objects (DTOs)
Strict separation between **External API Models** (TMDb responses) and **Internal DTOs**. This ensures that changes in the TMDb API won't break the mobile or web clients consuming this API.

---

## 🛠️ Tech Stack
- **Framework:** .NET 9 (ASP.NET Core)
- **Resilience:** Polly (Microsoft.Extensions.Http.Resilience)
- **Logging:** Serilog
- **Documentation:** Swagger / OpenAPI
- **Caching:** IMemoryCache (Local)

---

## 📋 Roadmap & Future Enhancements
- [ ] **Distributed Caching:** Transition from `IMemoryCache` to **Redis** (`IDistributedCache`) for better scalability in multi-node environments.
- [ ] **Server-side Filtering:** Optimize performance by using specific TMDb endpoints for Movies vs. TV Shows instead of in-memory filtering.
- [ ] **Rate Limiting:** Protect the API from abuse by implementing global and client-specific rate limits.
- [ ] **Unit Testing:** Increase code coverage using **xUnit** and **Moq** for the Application layer.

---

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- A valid [TMDb API Read Access Token](https://www.themoviedb.org/settings/api)

### Installation
1. Clone the repository.
2. Open `appsettings.json` and add your TMDb Token:
   ```json
   "Tmdb": {
     "BaseUrl": "[https://api.themoviedb.org/3/](https://api.themoviedb.org/3/)",
     "ApiToken": "YOUR_BEARER_TOKEN_HERE"
   }