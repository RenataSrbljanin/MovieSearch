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
* **Manual Invalidation** via **Webhooks**.

## 🚀 Features & Infrastructure
- **Observability & Monitoring:** 
    * **Prometheus Integration:** Custom middleware for capturing HTTP request duration, count, and error rates. Exposed via `/metrics` endpoint using `prometheus-net`.
    * **Grafana Dashboards:** Ready-to-use visualizations for real-time tracking of memory usage (`dotnet-counters`), request latency, and throughput.
    * **ServiceMonitor (Operator pattern):** Automated discovery of the API by Prometheus within the Kubernetes cluster.
- **Health Checks:** Comprehensive health monitoring (API, Redis, and Database).
- **Containerization:** Fully Dockerized and orchestrated with Kubernetes (K8s).

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

### 🔑 Local Security & Configuration
This project uses **User Secrets** to protect sensitive credentials during development.

### 🐳 Containerization (Docker)
* **Multi-stage Build:** Uses SDK image for compiling and a lightweight ASP.NET runtime image for production.
* **Port Configuration:** Pre-configured to run on port **8080**.

### ⚙️ CI/CD & Automation (GitHub Actions)
Fully automated development lifecycle managed via GitHub Actions:
* **Continuous Integration:** Every push and pull request triggers an automated suite of unit and integration tests.
* **Automated Quality Gate:** Build and test execution ensures that no breaking changes reach the main branch.
* **Environment Simulation:** The pipeline securely injects cryptographic keys and API tokens via GitHub Secrets to mirror production environments.
* **Docker Image Validation:** Verifies the multi-stage build process and container integrity as part of the pipeline.

### ☸️ Cloud-Native & Kubernetes Orchestration
The application is fully prepared for modern cloud environments with a complete Kubernetes manifest suite:
* **Orchestrated Deployment:** Managed via K8s with strategic resource requests and limits.
* **Horizontal Pod Autoscaler (HPA):** Configured to automatically scale from **2 to 10 replicas** based on real-time CPU utilization.
* **Professional Routing (Ingress):** Implemented Nginx Ingress to provide a clean domain-based entry point (`moviesearch.local`).
* **Self-Healing & High Availability:** Multi-replica setup ensures zero downtime and automatic recovery.
* **Monitoring as Code:** Includes a `ServiceMonitor` manifest, enabling the Prometheus Operator to automatically scrape metrics without manual configuration.
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
* **Orchestration:** Kubernetes (K8s)
* **Ingress/Routing:** Nginx Ingress Controller

---

## 🚀 Getting Started

### 🚀 Prerequisites

To run this project in all modes (Local and Kubernetes), you will need the following:

* **.NET 9 SDK** (for local development and building)
* **Docker Desktop** with **Kubernetes enabled** (for containerization and orchestration)
* **TMDb API Key** (to access movie and TV show data)
* **Upstash Redis Account** (for distributed caching)
* **Kube-Prometheus-Stack** (installed via Helm in the 'monitoring' namespace)
* **Metrics Server** (installed within the K8s cluster, required for HPA to function)

### 🔑 Local Security & Configuration
This project uses **User Secrets** to protect sensitive credentials during development.

1. **Initialize User Secrets:**
   ``Bash 
dotnet user-secrets init --project src/MovieSearch.Api

2. **Configure User Secrets:**  
dotnet user-secrets set "Tmdb:ApiToken" "your_tmdb_token"
dotnet user-secrets set "ConnectionStrings:RedisConnection" "your_upstash_url"
dotnet user-secrets set "Jwt:Key" "your_secret_key"
dotnet user-secrets set "WebhookOptions:ApiKey" "your_webhook_api_key"

### ☸️ Kubernetes Deployment (Local)
To run the full infrastructure locally on Docker Desktop:

1. **Create Kubernetes Secrets**
Inject your sensitive data into the cluster:

```PowerShell
kubectl create secret generic moviesearch-secrets \
  --from-literal=TMDB_TOKEN="your_token" \
  --from-literal=REDIS_CONN="your_upstash_url" \
  --from-literal=JWT_KEY="your_key" \
  --from-literal=WEBHOOK_KEY="your_webhook_key"
  ```

2. **Configure Local DNS**
Add the following line to your hosts file (C:\Windows\System32\drivers\etc\hosts):

```Plaintext
127.0.0.1  moviesearch.local
```

3. **Deploy to Cluster**
Apply all manifests from the k8s/ directory:

```PowerShell
kubectl apply -f k8s/
```
4. **Verify Access & Monitoring**
Once the stack is up, you can verify the status and monitoring data using these endpoints:

API Health: http://moviesearch.local/health (Aggregated status of API, Redis, and Database)

Swagger UI: http://moviesearch.local/swagger (Interactive documentation)

Prometheus Metrics: http://moviesearch.local/metrics (Scrapable data for Prometheus)

Auto-scaling (HPA): Run `kubectl get hpa`  to see real-time CPU-based scaling metrics.

Cluster Targets: Check http://localhost:9090/targets (via port-forward) to ensure the ServiceMonitor is active.

### 📊 Accessing Monitoring Dashboards
Once the application is deployed to Kubernetes, you can access the monitoring stack using port-forwarding:

 **Prometheus UI (Targets & Queries):**

   ```PowerShell'
   kubectl port-forward svc/prometheus-stack-kube-prom-prometheus 9090:9090 -n monitoring
   ```
   Access at: http://localhost:9090/targets
  ```PowerShell'
  kubectl port-forward deployment/prometheus-stack-grafana 3000:3000 -n monitoring
   ```
   Access at: http://localhost:3000 (Default credentials: admin / prom-operator)
  ```PowerShell'
   kubectl port-forward svc/moviesearch-service 8081:80
   ```
   Check raw metrics at: http://localhost:8081/metrics
   

## 📋 Roadmap & Future Enhancements

### 🚀 Phase 2: Hybrid Data Persistence & Caching
Evolving the project`s infrastructure by introducing permanent storage layers while **maintaining and enhancing the existing Redis caching strategy**. The goal is to move from ephemeral storage to a robust, multi-tier data architecture.

#### 🐘 PostgreSQL (Structured Identity & Core Data)
*   **Role:** Primary Source of Truth for relational data.
*   **Implementation:** Moving away from hardcoded "admin" logic to a secure, persistent user management system using **Entity Framework Core**.
*   **Key Features:** User profiles, encrypted credentials, and personalized "Watchlists".
*   **Development:** Managed via **Docker Compose** for a consistent local development environment.

#### 🍃 MongoDB (Unstructured Metadata Archive)
*   **Role:** High-performance document store for semi-structured data.
*   **Implementation:** Storing rich movie metadata and TMDB API responses to minimize external API latency and rate-limiting issues.
*   **Synergy:** Acts as a secondary persistent cache for complex objects that don`t fit well in a relational schema.

#### ⚡ Redis (Distributed Cache - *Existing*)
*   **Role:** High-speed volatile storage.
*   **Current Use:** Managing **Refresh Tokens** and session-based data.
*   **Enhancement:** Redis will continue to serve as the primary caching layer to ensure sub-millisecond response times, working in tandem with PostgreSQL and MongoDB to reduce database load.

### 🛠 Infrastructure & Orchestration
- [ ] **Multi-Container Setup:** Create a `docker-compose.yml` to orchestrate the API, Redis, and PostgreSQL for seamless onboarding.
- [ ] **Kubernetes Evolution:** Update `k8s` manifests to include `StatefulSets` for databases and `Secrets` for secure connection string management.
- [ ] **Repository Pattern Refactor:** Decouple data access logic in the `Infrastructure` layer to support multiple data sources.

### 🔍 Advanced Features
- [ ] **Smart Tiered Caching:** Implement a logic where the app checks Redis (L1), then MongoDB (L2), and finally TMDB API (L3).
- [ ] **Audit Logging:** Store incoming webhook events in PostgreSQL for historical tracking and debugging.


