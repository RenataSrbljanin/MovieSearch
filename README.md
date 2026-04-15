# 🎬 MovieSearch API (.NET 9)
![Build Status](https://github.com/RenataSrbljanin/MovieSearch/actions/workflows/ci-cd.yml/badge.svg)

A professional, production-ready .NET 9 Web API for searching movies and TV shows, integrated with [The Movie Database (TMDb) API](https://www.themoviedb.org/). This project demonstrates advanced software engineering practices, focusing on performance, security, and scalability.

---

## 🌟 Key Features & Architecture

### 🏛️ Clean Architecture Implementation
The project follows **Clean Architecture** principles to ensure a high degree of decoupling:
* **Api Layer:** Global Error Handling Middleware, Health Checks, and REST Controllers.
* **Application Layer:** Business logic, DTO mapping, and Service abstractions.
* **Infrastructure Layer:** High-performance integration handling external dependencies:
    * **Typed HTTP Clients:** Developed a robust `ITmdbClient` that encapsulates all logic for external communication. It supports specialized endpoints (`/search/movie`, `/search/tv`, `/search/multi`) to ensure **server-side filtering** and **accurate pagination**.
    * **Redis Configuration (Distributed Cache):** Implemented using **Upstash Redis** to move away from limited in-memory caching. This ensures the application is stateless and ready for horizontal scaling.
    * **Resilience & Fault Tolerance:** Integrated **Polly** within the HTTP pipeline, providing automatic retries and circuit-breaking.

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
* **Caching:** Redis (Upstash)
* **Resilience:** Polly
* **Logging:** Serilog
* **Testing:** xUnit, Moq
* **Documentation:** Swagger / OpenAPI
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


## 📋 Roadmap & Future Enhancements

* API Versioning: Implement formal API versioning (v1, v2) to ensure backward compatibility for future updates.    

* Webhook - Event-driven cache invalidation

* Advanced Monitoring: Deploy Prometheus and Grafana dashboards for real-time API metrics and Redis health visualization.

* Orchestration: Add Kubernetes (K8s) manifests for automated scaling and self-healing deployments.
