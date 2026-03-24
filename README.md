# Movie Search API – DEPT® Assessment Solution

This repository contains a high-performance .NET 9 Web API designed to search for movie and TV show trailers. 
Built with scalability and global presence in mind, it integrates with **The Movie Database (TMDB)** and provides a unified experience for movie enthusiasts.

## 🏗 Architectural Decisions

### 1. Clean Architecture & SRP
The project follows a decoupled structure (API, Application, Infrastructure) to ensure maintainability. 
Business logic is separated from external service implementations, making it "future-proof" for provider swaps (e.g., switching from YouTube to Vimeo).

### 2. Scalability & Performance
- **Parallel Task Execution**: In `MovieDetailsService`, metadata and video data are fetched in parallel using `Task.WhenAll`, reducing response times by up to 50%.
- **Efficient Caching Strategy**: 
    - **Absolute Expiration (10 min)**: Ensures data freshness.
    - **Sliding Expiration (2 min)**: Frees up RAM by removing inactive search results, crucial for global scalability.
- **Typed HttpClient**: Implemented via `IHttpClientFactory` to prevent *Socket Exhaustion* and optimize connection pooling.

### 3. Security (OWASP Top 10)
- **Safe Inputs**: All user queries are sanitized using `Uri.EscapeDataString` to prevent injection.
- **Information Exposure**: A global `ErrorHandlingMiddleware` intercepts exceptions, providing clean responses to the client while logging details internally.
- **Secure Configuration**: External API secrets are managed via the Options pattern, ready for Azure Key Vault or AWS Secrets Manager integration.

---
## 🚀 Key Features

- **Unified Search**: Search across movies and TV shows simultaneously using a single endpoint.
- **Detailed Media Insights**: Comprehensive detail pages including overviews, ratings, genres, and direct YouTube trailer links.
- **Structured Logging:** Integrated **Serilog** for both Console and File logging with daily rolling intervals.
- **Global Error Handling:** Custom middleware intercepts exceptions, providing clean responses while logging full stack traces internally.
- **Dockerized:** Container-ready with a multi-stage `Dockerfile` for consistent deployment across environments.
- **High-Performance Caching**: Intelligent in-memory caching to minimize external API latency and costs.
- **Resilient Architecture**: Built using modern C# patterns to ensure stability under high load.

---
## 🛠 Tech Stack
- **Framework**: .NET 9.0
- **Documentation**: Swagger/OpenAPI (with custom metadata)
- **Tools**: `System.Text.Json` (Stream-based deserialization for low memory footprint)
- **Logging:** Serilog
- **Deployment:** Docker
- **Pattern**: Repository/Service pattern with Dependency Injection

---

## ⚙️ Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A **TMDB API Read Access Token** (Bearer Token)

### ⚙️ Setup & Run

#### 1. Clone the repository
```bash
git clone https://github.com/RenataSrbljanin/MovieSearch.git
cd MovieSearch
```

#### 2. Configuration
Open `src/MovieSearch.Api/appsettings.json` and replace the `ApiToken` value with your actual TMDB Bearer Token:
```json
"Tmdb": {
  "BaseUrl": "https://api.themoviedb.org/3/",
  "ApiToken": "YOUR_BEARER_TOKEN_HERE"
}
```

#### 3. Run with .NET CLI
Navigate to the API project folder and run:
```bash
cd src/MovieSearch.Api
dotnet run
```
The API will be available at `http://localhost:5000`. Access **Swagger UI** at `/swagger`.

---
## 🐳 Docker Support
To run the application as a containerized service:

1. **Build the image:**
   ```bash
   docker build -t moviesearch-api .
   ```

2. **Run the container:**
   ```bash
   docker run -p 8080:8080 moviesearch-api
   ```
*The API will be accessible at `http://localhost:8080/swagger`.*

---
## 📝 Logging & Monitoring
- **Serilog** provides detailed insights into application health.
- Logs are output to the **Console** for real-time debugging and persisted in the `/logs` directory for production monitoring.
- **Error Handling Middleware** logs all unhandled exceptions automatically, including the request path and full exception details.

---

  