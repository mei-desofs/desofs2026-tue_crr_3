# Tea Shop API

**SonarCloud:** [mei-desofs-1_tue-crr-3](https://sonarcloud.io/project/overview?id=mei-desofs-1_tue-crr-3)

---

## 0. Running Locally

### Prerequisites

| Tool | Version                                  |
|------|------------------------------------------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0                                     |
| [PostgreSQL](https://www.postgresql.org/download/) | 14+                                      |
| [Docker + Docker Compose](https://docs.docker.com/get-docker/) | 24+ *(optional — for containerized run)* |

### Setup (direct)

```bash
# 1. Clone the repository
git clone https://github.com/mei-desofs/desofs2026-tue_crr_3.git
cd desofs2026-tue_crr_3/TeaShop

# 2. Supply secrets (never committed to source control)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Database=teashop;Username=<user>;Password=<pass>" \
  --project TeaShop

dotnet user-secrets set "Seed:AdminEmail"    "admin@teashop.local" --project TeaShop
dotnet user-secrets set "Seed:AdminPassword" "<strong-password>"   --project TeaShop

# 3. Apply database migrations
dotnet ef database update --project TeaShop

# 4. Run the API  (Swagger UI → http://localhost:5000/swagger)
dotnet run --project TeaShop
```

### Setup (Docker Compose)

```bash
# Secrets are injected via environment variables — see docker-compose.yml
  docker compose up --build
```

> The Docker image runs the application as a non-root user. See [Dockerfile](TeaShop/Dockerfile).

### Running the tests

```bash
dotnet test TeaShop.Test/TeaShop.Test.csproj
```

All 225tests should pass. The suite includes unit tests, domain tests, and integration tests.

---

## 1. Development

### 1.1 API Endpoints

| Aggregate    | Method | Endpoint                   | Roles permitted          |
|--------------|--------|----------------------------|--------------------------|
| **Auth**     | POST   | `/api/auth/signUp`         | Public                   |
|              | POST   | `/api/auth/login`          | Public                   |
|              | POST   | `/api/auth/logout`         | Authenticated            |
|              | POST   | `/api/auth/change-password` | Authenticated            |
|              | POST   | `/api/staff`               | Admin                    |
| **Address**  | GET    | `/api/users/address`       | CUSTOMER, MANAGER, ADMIN |
|              | PUT    | `/api/users/address`       | CUSTOMER, MANAGER, ADMIN |
|              | DELETE | `/api/users/address`       | CUSTOMER, MANAGER, ADMIN |
| **Catalog**  | GET    | `/api/catalog`             | Public                   |
|              | GET    | `/api/catalog/{id}`        | Public                   |
|              | PUT    | `/api/catalog/{id} `       | MANAGER, ADMIN           |
|              | DELETE | `/api/catalog/{id}`        | MANAGER,ADMIN            |
|              | POST   | `/api/catalog/`            | Public                   |
|              | POST   | `/api/catalog/{id}/image`  | MANAGER,ADMIN                    |
|              | GET    | `/api/catalog/{id}/image`            | MANAGER,ADMIN                    |
|              | DELETE | `/api/catalog/{id}/image`            | MANAGER,ADMIN                    |
| **Category** | POST   | `/api/categories`          |  ADMIN           |
|              | PATCH  | `/api/categories/{id}`     | ADMIN                    |
|              | DELETE | `/api/categories/{id}`     | ADMIN                    |
| **Order**    | POST   | `/api/orders`              | CUSTOMER, MANAGER, ADMIN |
|              | GET    | `/api/orders/me`           | CUSTOMER, MANAGER, ADMIN |
|              | PATCH  | `/api/orders/{id}/cancel`  | CUSTOMER, MANAGER, ADMIN |
|              | GET    | `/api/orders`              | ADMIN                    |
|              | PUT    | `/api/orders/{id}/status`  | ADMIN                    |
|              | POST   | `/api/orders/export`       | ADMIN                    |



Authorization enforces three distinct roles: `CUSTOMER`, `MANAGER`, `ADMIN`.

---

### 1.2 Security Controls

| Control | Evidence                                                                                              |
|---------|-------------------------------------------------------------------------------------------------------|
| Parameterized queries via EF Core — no raw SQL with user input | [UserRepository.cs](TeaShop/TeaShop/Infrastructure/Persistence/Repositories/UserRepository.cs)        |
| Session tokens generated with `RandomNumberGenerator` (256-bit entropy), SHA-256 hashed at rest | [SessionToken.cs](TeaShop/TeaShop/Domain/IAM/SessionToken.cs)                                         |
| Role-based authorization enforced at the controller layer via `[Authorize(Roles = "...")]` | [OrderController.cs](TeaShop/TeaShop/Presentation/OrderController.cs)                                 |
| Password policy: minimum 15 characters, checked against HaveIBeenPwned, blocks common patterns | [PasswordPolicyChecker.cs](TeaShop/TeaShop/Infrastructure/Security/PasswordPolicyChecker.cs)          |
| Account lockout after 5 consecutive failed login attempts  | [Program.cs](TeaShop/TeaShop/Program.cs)                                                              |
| Timing-attack-safe login — dummy hash computed even when the user does not exist | [AuthService.cs](TeaShop/TeaShop/Application/Auth/AuthService.cs)                                     |
| Rate limiting on auth endpoints  and general endpoints  | [RateLimiting.cs](TeaShop/TeaShop/Infrastructure/Security/RateLimiting.cs)                            |
| DTO input validation — `[Range]`, `[Required]`, `[MaxLength]` enforced at HTTP binding layer | [OrderDTOs.cs](TeaShop/TeaShop/Application/Orders/OrderDTOs.cs)                                       |
| Stock decrement and order creation wrapped in a DB transaction — prevents partial commits under concurrent requests | [OrderService.cs](TeaShop/TeaShop/Application/Orders/OrderService.cs)                                 |
| Order ownership enforced at the domain level — users can only cancel their own orders (BOLA mitigation) | [Order.cs](TeaShop/TeaShop/Domain/Orders/Order.cs)                                                    |
| Structured security logging — authentication events, authorization denials, file operations without logging credentials or PII | [SecurityLogger.cs](TeaShop/TeaShop/Infrastructure/Logging/SecurityLogger.cs)                         |
| File upload validation — extension allowlist, magic-bytes check, and maximum size enforced; uploaded files stored outside the web root and not served as executable code |[FileUploadService.cs](TeaShop/TeaShop/Infrastructure/Services/FileUploadService.cs)                   |
| Security response headers — `Content-Security-Policy`, `X-Content-Type-Options: nosniff`, `Referrer-Policy`, `Strict-Transport-Security` | [SecurityHeadersMiddleware.cs](TeaShop/TeaShop/Infrastructure/Middleware/SecurityHeadersMiddleware.cs) |
| No secrets in source control — connection string and seed credentials loaded from .NET User Secrets locally, environment variables in production | [TeaShop.csproj](TeaShop/TeaShop/TeaShop.csproj) · [docker-compose.yml](docker-compose.yml)           |
| Generic error responses — no stack traces exposed to clients | [GenericExceptionMiddleware.cs](TeaShop/TeaShop/Infrastructure/Middleware/GenericExceptionMiddleware.cs) |
| Log Injection Mitigation  — stripping of carriage returns, line feeds, and control characters from untrusted input before logging | [GenericExceptionMiddleware.cs](TeaShop/TeaShop/Infrastructure/Middleware/GenericExceptionMiddleware.cs)|
---

### 1.3 Code Reviews

All code changes were merged to the `main` branch via Pull Requests. Each merge required:

- **Automated Validation:** Successful completion of the CI/CD pipeline (build, test, SAST, coverage).
- **Mandatory Review:** Explicit manual review by a team member before merge.

---

### 1.4 Static Analysis (SAST)

Two SAST tools run automatically on every push and pull request.

**SonarCloud** — integrated in the CI pipeline (`.github/workflows/ci.yml`):

- Pipeline: `build → Sonar Begin → build → Test with Coverage → Sonar End`
- Dashboard: [mei-desofs-1_tue-crr-3 on SonarCloud](https://sonarcloud.io/project/overview?id=mei-desofs-1_tue-crr-3)
- [Example run](https://github.com/mei-desofs/desofs2026-tue_crr_3/actions/runs/25952080145/job/76291798673)

**CodeQL** — runs via GitHub Advanced Security on every push to `main` and every PR:

- [Code Scanning results](https://github.com/mei-desofs/desofs2026-tue_crr_3/security/code-scanning)

No unresolved HIGH or CRITICAL findings remain open.

---

### 1.5 Software Composition Analysis & Secret Scanning (SCA)
We use two automated tools to detect vulnerable dependencies and hardcoded secrets:

**GitHub Dependency Review Action** — runs on every PR targeting `main` and is configured to **fail the pipeline** on any dependency with a HIGH or CRITICAL CVE, blocking the merge.

```yaml
- uses: actions/dependency-review-action@v4
  with:
    fail-on-severity: high
```

**Trivy (Aqua Security)** — configured as a filesystem and secret scanner in the CI pipeline (`security.yml`). On every push, Trivy scans the raw repository to:
- Identify vulnerable NuGet packages.
- Scan for committed secrets (such as hardcoded API keys, connection strings, or private keys) across the codebase.
- The pipeline is configured to fail on any HIGH or CRITICAL findings.

```yaml
 - uses: aquasecurity/trivy-action@v0.36.0
   with:
          scan-type: fs
          scan-ref: TeaShop/publish
          scanners: vuln,secret
          severity: HIGH,CRITICAL
          exit-code: '1'
          format: sarif
          output: trivy-artifact.sarif     
```
---

## 2. Build and Test

### 2.1 Component Inventory (SBOM)

A Software Bill of Materials is generated automatically on every push and PR by the `sbom` job in `security.yml` using **CycloneDX** (`dotnet-CycloneDX`). The output is a JSON SBOM uploaded as a pipeline artifact.

[Example run with SBOM artifact](https://github.com/mei-desofs/desofs2026-tue_crr_3/actions/runs/25990721159)

---

### 2.2 Test Execution

Tests run as part of the CI pipeline. Coverage is collected with `dotnet-coverage` and reported to SonarCloud.

**Current totals: 225 tests, 0 failures.**

| Category                  | Location |
|---------------------------|----------|
| Domain unit tests         | `TeaShop.Test/Unit/Domain/` |
| Application service tests | `TeaShop.Test/Unit/Application/` |
| Infrastructure tests      | `TeaShop.Test/Unit/Infrastructure/` |
| Integration tests         | `TeaShop.Test/Integration/` |

[Test project](TeaShop/TeaShop.Test)

---

### 2.3 Dynamic Analysis (DAST)

**OWASP ZAP API Scan** runs on every push to `main` (`security.yml`, job `dynamic_analysis`):

1. Starts the API on port `8080` in `CI` environment mode
2. Polls until the application is ready
3. Runs `zaproxy/action-api-scan@v0.9.0` against the OpenAPI spec at `/openapi/v1.json`
4. Uploads the full ZAP report as the `zap-api-report` artifact; **pipeline fails on any unaccepted finding**

[Example run with ZAP report](https://github.com/mei-desofs/desofs2026-tue_crr_3/actions/runs/25990721159)

---

### 2.4 Configuration Validation

| Check                                                                             | Evidence                                             |
|-----------------------------------------------------------------------------------|------------------------------------------------------|
| No secrets in `appsettings.json`                                                  | [appsettings.json](TeaShop/TeaShop/appsettings.json) |
| Swagger UI, developer exception pages, and detailed errors disabled in production | [Program.cs](TeaShop/TeaShop/Program.cs)             |
| `.git` folder excluded from Docker image                                          | [.dockerignore](TeaShop/.dockerignore)               |

---

## 3. Pipeline Automation

| Stage                                | Tool                           | Trigger                | File           | Automated  |
|--------------------------------------|--------------------------------|------------------------|----------------|------------|
| Build & Tests                        | `dotnet build` + `dotnet test` | Push & PR to `main`    | `ci.yml`       | ✅          |
| Code Coverage                        | `dotnet-coverage` → SonarCloud | Push & PR to `main`    | `ci.yml`       | ✅          |
| SAST — Semantic analysis             | SonarCloud                     | Push & PR to `main`    | `ci.yml`       | ✅          |
| SAST — Deep injection/path analysis  | CodeQL                         | Push & PR to `main`    | GitHub auto    | ✅          |
| SCA — Dependency vulnerability check | Dependency Review              | PR to `main` only      | `security.yml` | ✅          |
| SCA & Secret Scan                    | Trivy                          | Push & PR to `main`    | `security.yml` | ✅          |
| Component Inventory (SBOM)           | CycloneDX                      | Push & PR to `main`    | `security.yml` | ✅          |
| DAST — Active API scan               | OWASP ZAP                      | Push to `main` only    | `security.yml` | ✅          |
| Scheduled full security scan         | All security jobs              | Weekly (Mon 03:00 UTC) | `security.yml` | ✅          |

→ [ci.yml](.github/workflows/ci.yml) · [security.yml](.github/workflows/security.yml)

---

## 4. ASVS Assessment

**Target level:** ASVS 5.0 — Level 2

Each compliant requirement in the tracker links to the relevant source file or documentation section. 

| Chapter | Applicability |
|---------|---------------|
| V10 OAuth and OIDC | N/A — application uses its own session-based IAM; no OAuth or OIDC flows |
| V17 WebRTC | N/A — no real-time media communication |
| V4.3 GraphQL | N/A — REST API only |

→ [ASVS\_5\_0\_Tracker.xlsx](Deliverables/ASVS_5_0_Tracker.xlsx)
