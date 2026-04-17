# 🎬 MovieSearch API (.NET 9)
![Build Status](https://github.com/RenataSrbljanin/MovieSearch/actions/workflows/ci-cd.yml/badge.svg)

A professional, production-ready .NET 9 Web API for searching movies and TV shows, integrated with [The Movie Database (TMDb) API](https://www.themoviedb.org/). This project demonstrates advanced software engineering practices, focusing on performance, security, and scalability.

---

## 🌟 Key Features & Architecture

### 🏛️ Clean Architecture Implementation
The project follows **Clean Architecture** principles to ensure a high degree of decoupling:
* **Api Layer:** Global Error Handling Middleware, Health Checks, **API Versioning (v1, v2)**, and REST Controllers.
* **Application Layer:** Business logic, DTO mapping, and Service abstractions.
* **Infrastructure Layer:** High-performance integration handling external dependencies:
    * **Typed HTTP Clients:** Developed a robust `ITmdbClient` that encapsulates all logic for external communication. It supports specialized endpoints (`/search/movie`, `/search/tv`, `/search/multi`) to ensure **server-side filtering** and **accurate pagination**.
    * **Generic Caching Abstraction**: Implemented a custom ICacheService to decouple the Application layer from specific caching providers. This architecture allows for seamless transitions between different distributed cache engines without affecting business logic.
    * **Redis Configuration (Distributed Cache):** Powered by **Upstash Redis** and implemented by `RedisCacheService` to move away from limited in-memory caching. This ensures the application is stateless, supports complex object serialization, and is fully optimized for horizontal scaling and cloud-native environments.
    * **Resilience & Fault Tolerance:** Integrated **Polly** within the HTTP pipeline, providing automatic retries and circuit-breaking to handle transient failures of external APIs gracefully..

### 🎯 Server-Side Filtering & Accurate Pagination
Optimized the search functionality by moving away from in-memory filtering:
* **Precise Results:** The API communicates with specific TMDb endpoints based on the user's request.
* **Pagination Integrity:** Ensures paged results are always consistent and full, resolving issues where in-memory filtering would return incomplete pages.

### 🚀 Distributed Caching (Redis)
* **Horizontal Scalability:** Multiple API instances share the same cache via Redis.
* **Optimized Performance:** Drastically reduces latency and TMDb API rate-limit consumption.

### 🛡️ Resilience & Fault Tolerance (Polly)
* **Standard Resilience Handler:** Includes **Retry, Circuit Breaker, and Rate Limiting** to ensure the app remains responsive even when external services "stutter".

### 🧪 Robust Testing Suite
* **Frameworks:** xUnit & Moq.
* **Coverage:** 
    * **Identity Services:** Validating JWT generation, Claims (Sub, Jti, Issuer), and Refresh Token rotation logic.
    * **Controllers:** Ensuring correct HTTP responses (200 OK, 401 Unauthorized) and proper API contract adherence.
    * **Infrastructure:** Caching logic, API fallback mechanisms, and data transformation.

### 📜 Secure Error Handling & Logging
* **Global Middleware:** Captures all exceptions, returning standardized JSON responses.
* **Serilog:** Structured logging for internal tracking while keeping public responses safe from information leakage.

### 🔐 Advanced Secure Authentication (JWT & Refresh Tokens)
Implemented a robust authentication system designed for stateless, distributed environments:
* **Dual-Token System:** Leverages short-lived Access Tokens for security and long-lived Refresh Tokens for a seamless user experience.
* **Distributed Token Store (Redis):** Refresh Tokens are persisted in **Upstash Redis** with an absolute expiration (TTL), ensuring the API remains stateless and ready for horizontal scaling.
* **One-Time Use Policy:** Implemented a secure rotation strategy where Refresh Tokens are revoked immediately upon use (One-Time Use), mitigating potential replay attacks.
* **Cryptographic Security:** Utilizes high-entropy, cryptographically generated keys for JWT signing, managed via .NET User Secrets.
* **Swagger Integration:** Fully configured Swagger UI to support JWT Bearer tokens, allowing for seamless testing of protected endpoints directly from the browser.

### 🚥 API Versioning
The API implements formal versioning to ensure backward compatibility and smooth transitions for future updates:
* **URL-Based Versioning:** All endpoints are prefixed with the version number (e.g., `/api/v1/auth/login`).
* **Format:** Uses the `v{version:apiVersion}` constraint for clean and predictable routing.
* **Extensibility:** Built with `Asp.Versioning`, allowing multiple versions of the same controller to coexist during transition periods.

### 🔗 Webhooks & Cache Invalidation
The API supports event-driven cache invalidation to ensure data consistency:
* **Endpoint:** `POST /api/v1/Webhooks/cache-invalidate/{type}/{id}/{language}`
* **Security:** Protected via `X-Api-Key` header validation.
* **Functionality:** Manually clears specific Redis keys for movies or TV shows, forcing the API to fetch fresh data from TMDB on the next request.

#### 🔑 Environment Variables & Secrets
To run this project locally, you need to configure the following secrets:
* `Tmdb:ApiToken`: Your TMDB Bearer Token.
* `WebhookOptions:ApiKey`: A custom secret key for protecting webhook endpoints.

Use `dotnet user-secrets set` to configure these values during development.

### 🐳 Containerization (Docker)
* **Multi-stage Build:** Uses SDK image for compiling and a lightweight ASP.NET runtime image for production.
* **Port Configuration:** Pre-configured to run on port **8080**.

### ⚙️ CI/CD & Automation (GitHub Actions)
Fully automated development lifecycle managed via GitHub Actions:
* **Continuous Integration:** Every push and pull request triggers an automated suite of unit and integration tests.
* **Automated Quality Gate:** Build and test execution ensures that no breaking changes reach the main branch.
* **Environment Simulation:** The pipeline securely injects cryptographic keys and API tokens via GitHub Secrets to mirror production environments.
* **Docker Image Validation:** Verifies the multi-stage build process and container integrity as part of the pipeline.
---

## 🛠️ Tech Stack
* **Framework:** .NET 9 (ASP.NET Core)
* **Security:** JWT Authentication (Bearer) & API Key Authorization (Webhooks).
* **Caching:** Redis with a custom generic `ICacheService` abstraction (Upstash)
* **Resilience:** Polly
* **Logging:** Serilog
* **Testing:** xUnit, Moq
* **Documentation:** Swagger / OpenAPI
* **Versioning:** Asp.Versioning (MVC & ApiExplorer)
* **CI/CD:** GitHub Actions
* **Containerization:** Docker

---

## 🚀 Getting Started

### Prerequisites
* .NET 9 SDK
* TMDb API Key
* Upstash Redis account

### Security & Configuration
This project uses **User Secrets** to protect sensitive credentials.

1. **Initialize User Secrets:**
   ```bash
   dotnet user-secrets init --project src/MovieSearch.Api

#### 🔑 Environment Variables & Secrets
To run this project locally, you need to configure the following secrets:
* `Tmdb:ApiToken`: Your TMDB Bearer Token.
* `WebhookOptions:ApiKey`: A custom secret key for protecting webhook endpoints.

Use `dotnet user-secrets set` to configure these values during development.

## 📋 Roadmap & Future Enhancements

* Advanced Monitoring: Deploy Prometheus and Grafana dashboards for real-time API metrics and Redis health visualization.

* Orchestration: Add Kubernetes (K8s) manifests for automated scaling and self-healing deployments.
