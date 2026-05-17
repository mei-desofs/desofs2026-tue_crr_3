# Phase 2 – Sprint 1 Deliverable

**Project:** Tea Shop API  
**SonarCloud:** [mei-desofs-1_tue-crr-3](https://sonarcloud.io/project/overview?id=mei-desofs-1_tue-crr-3)

---

## 0. Running Locally

### Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |
| [PostgreSQL](https://www.postgresql.org/download/) | 14+ |

### Setup

```bash
# 1. Clone the repository
git clone https://github.com/mei-desofs/desofs2026-tue_crr_3.git
cd desofs2026-tue_crr_3/TeaShop

# 2. Supply secrets (never committed to source control)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=teashop;Username=<user>;Password=<pass>" --project TeaShop

dotnet user-secrets set "Seed:AdminEmail"    "admin@teashop.local" --project TeaShop
dotnet user-secrets set "Seed:AdminPassword" "<strong-password>"   --project TeaShop

# 3. Apply database migrations
dotnet ef database update --project TeaShop

# 4. Run the API  (Swagger UI → http://localhost:5000/swagger)
dotnet run --project TeaShop
```

### Running the tests

```bash
dotnet test TeaShop.Test/TeaShop.Test.csproj
```

All 176 tests should pass. The suite includes unit tests, domain tests, repository tests, and integration tests.

---

## 1. Development

### 1.1 Implemented Functionality

The following aggregates and their primary endpoints were implemented this sprint:

| Aggregate    | Method   | Endpoint                          | Roles permitted            |
|--------------|----------|-----------------------------------|----------------------------|
| **Auth**     | POST     | `/api/auth/signUp`                | Public                     |
|              | POST     | `/api/auth/login`                 | Public                     |
|              | POST     | `/api/auth/logout`                | Authenticated              |
|              | POST     | `/api/auth/change-password`       | Authenticated              |
| **Address**  | GET      | `/api/users/address`              | CUSTOMER, MANAGER, ADMIN   |
|              | PUT      | `/api/users/address`              | CUSTOMER, MANAGER, ADMIN   |
|              | DELETE   | `/api/users/address`              | CUSTOMER, MANAGER, ADMIN   |
| **Catalog**  | GET      | `/api/catalog`                    | Public                     |
|              | GET      | `/api/catalog/{id}`               | Public                     |
|              | PATCH    | `/api/catalog/{id}/stock`         | MANAGER, ADMIN             |
| **Category** | POST     | `/api/categories`                 | ADMIN                      |
|              | PATCH    | `/api/categories/{id}`            | ADMIN                      |
|              | DELETE   | `/api/categories/{id}`            | ADMIN                      |
| **Order**    | POST     | `/api/orders`                     | CUSTOMER, MANAGER, ADMIN   |
|              | GET      | `/api/orders/me`                  | CUSTOMER, MANAGER, ADMIN   |
|              | PATCH    | `/api/orders/{id}/cancel`         | CUSTOMER, MANAGER, ADMIN   |
|              | GET      | `/api/orders`                     | ADMIN                      |
|              | PUT      | `/api/orders/{id}/status`         | ADMIN                      |
| **IAM**      | —        | Internal session lifecycle        | Internal                   |

Authorization enforces three distinct roles: `CUSTOMER`, `MANAGER`, `ADMIN`.

---

### 1.2 Security Practices Adopted

The following security controls were deliberately implemented during development.

| Practice | Evidence |
|----------|----------|
| Parameterized queries via EF Core — no raw SQL with user input | [UserRepository.cs](TeaShop/TeaShop/Infrastructure/Persistence/Repositories/UserRepository.cs) |
| Session tokens generated with `RandomNumberGenerator` (256-bit entropy), SHA-256 hashed at rest | [SessionToken.cs](TeaShop/TeaShop/Domain/IAM/SessionToken.cs) |
| Role-based authorization enforced at controller layer via `[Authorize(Roles = "...")]` | [OrderController.cs](TeaShop/TeaShop/Presentation/OrderController.cs) |
| Password policy: minimum 15 characters, checked against HaveIBeenPwned, blocks common patterns | [PasswordPolicyChecker.cs](TeaShop/TeaShop/Infrastructure/Security/PasswordPolicyChecker.cs) |
| Account lockout after 5 consecutive failed login attempts (15-minute cooldown) | [User.cs](TeaShop/TeaShop/Domain/Users/User.cs) |
| Timing-attack-safe login — dummy hash computed even when user does not exist | [AuthService.cs](TeaShop/TeaShop/Application/Auth/AuthService.cs) |
| Rate limiting on auth endpoints (5 req/min per IP) and general endpoints (60 req/min per IP) | [RateLimiting.cs](TeaShop/TeaShop/Infrastructure/Security/RateLimiting.cs) |
| DTO input validation with data annotations — `[Range]`, `[Required]`, `[MaxLength]` enforced at HTTP binding layer | [OrderDTOs.cs](TeaShop/TeaShop/Application/Orders/OrderDTOs.cs) |
| Stock decrement and order creation wrapped in a DB transaction — prevents partial commits under concurrent requests | [OrderService.cs](TeaShop/TeaShop/Application/Orders/OrderService.cs) |
| Order ownership enforced at domain level — users can only cancel their own orders | [Order.cs](TeaShop/TeaShop/Domain/Orders/Order.cs) |
| No secrets in `appsettings.json` — connection string and seed credentials loaded from .NET User Secrets | [TeaShop.csproj](TeaShop/TeaShop/TeaShop.csproj) |
| Generic error responses in production — no stack traces exposed to clients | [GenericExceptionMiddleware.cs](TeaShop/TeaShop/Infrastructure/Middleware/GenericExceptionMiddleware.cs) |
| Domain value objects enforce invariants — prevents type confusion and invalid state | [Email.cs](TeaShop/TeaShop/Domain/Users/Email.cs) |

---

### 1.3 Code Reviews

All code changes were merged to the `main` branch via Pull Requests. Each merge required:

- **Automated Validation:** Successful completion of the CI/CD pipeline (build, test, SAST, coverage).
- **Mandatory Review:** Explicit manual review by a team member before merge.

---

### 1.4 Static Analysis (SAST)

Two SAST tools run automatically on every push and pull request.

**SonarCloud** is integrated in the CI pipeline (`.github/workflows/ci.yml`).

- Pipeline job: `build → Sonar Begin → build → Test with Coverage → Sonar End`
- Dashboard: [mei-desofs-1_tue-crr-3 on SonarCloud](https://sonarcloud.io/project/overview?id=mei-desofs-1_tue-crr-3)
- [Example successful pipeline run](https://github.com/mei-desofs/desofs2026-tue_crr_3/actions/runs/25952080145/job/76291798673)

**CodeQL** runs automatically via GitHub Advanced Security on every push to `main` and every pull request targeting `main`.

- [Code Scanning results](https://github.com/mei-desofs/desofs2026-tue_crr_3/security/code-scanning)

---

### 1.5 Software Composition Analysis (SCA)

Dependency security is enforced at the pull request level. The **GitHub Dependency Review Action** runs on every PR targeting `main` (`security.yml`, job `dependency-review`) and is configured to fail the pipeline on any dependency with **HIGH or CRITICAL** severity CVEs, blocking the merge.

```yaml
# .github/workflows/security.yml
- uses: actions/dependency-review-action@v4
  with:
    fail-on-severity: high
```

This directly mitigates supply chain risks against third-party libraries.

---

## 2. Build and Test

### 2.1 Component Inventory (SBOM)

A Software Bill of Materials is generated automatically on every push and pull request by the `sbom` job in `security.yml` using **CycloneDX** (`dotnet-CycloneDX`). The output is a JSON SBOM uploaded as the `sbom` pipeline artifact.

[Example pipeline run with SBOM artifact](https://github.com/mei-desofs/desofs2026-tue_crr_3/actions/runs/25990721159)

---

### 2.2 Test Execution

Tests run as part of the CI pipeline (`build` job, `ci.yml`). Coverage is collected with `dotnet-coverage` and reported to SonarCloud.

**Current totals: 176 tests, 0 failures.**

Test categories:

| Category | Location |
|----------|----------|
| Domain unit tests | `TeaShop.Test/Unit/Domain/` |
| Application service tests | `TeaShop.Test/Unit/Application/` |
| Infrastructure / repository tests | `TeaShop.Test/Unit/Infrastructure/` |
| Integration tests | `TeaShop.Test/Integration/` |

[Test project](TeaShop/TeaShop.Test)

---

### 2.3 Dynamic Analysis (DAST)

**OWASP ZAP API Scan** runs automatically on every push to `main` (`security.yml`, job `dynamic_analysis`). It:

1. Starts the API with `dotnet run` in `CI` environment mode on port `8080`
2. Polls `http://localhost:8080` until the application is ready
3. Runs `zaproxy/action-api-scan@v0.9.0` against the OpenAPI spec at `/openapi/v1.json`
4. Uploads the full ZAP report as the `zap-api-report` artifact

The scan is configured with `fail_action: false` so findings are reviewed without blocking the pipeline.

[Example pipeline run with ZAP report](https://github.com/mei-desofs/desofs2026-tue_crr_3/actions/runs/25990721159)

---

### 2.4 Configuration Validation

| Check | Evidence |
|-------|----------|
| No secrets committed to `appsettings.json` | [appsettings.json](TeaShop/TeaShop/appsettings.json) |
| Different settings for development and production environments | [Program.cs](TeaShop/TeaShop/Program.cs) |

---

## 3. Pipeline Automation

Both pipelines run automatically — no manual steps are required to trigger security checks.

| Stage | Tool | Trigger | Pipeline file | Automated |
|-------|------|---------|---------------|-----------|
| Build & Unit/Integration Tests | `dotnet build` + `dotnet test` | Push & PR to `main` | `ci.yml` | ✅ |
| Code Coverage | `dotnet-coverage` → SonarCloud | Push & PR to `main` | `ci.yml` | ✅ |
| SAST — Semantic analysis | SonarCloud | Push & PR to `main` | `ci.yml` | ✅ |
| SAST — Deep injection/path analysis | CodeQL (GitHub Advanced Security) | Push & PR to `main` | GitHub auto | ✅ |
| SCA — Dependency vulnerability check | Dependency Review | PR to `main` only | `security.yml` | ✅ |
| Component Inventory (SBOM) | CycloneDX | Push & PR to `main` | `security.yml` | ✅ |
| DAST — Active API scan | OWASP ZAP (`action-api-scan`) | Push to `main` only | `security.yml` | ✅ |
| Scheduled full security scan | All security jobs | Weekly (Mon 03:00) | `security.yml` | ✅ |

→ [ci.yml](.github/workflows/ci.yml) · [security.yml](.github/workflows/security.yml)

---

## 4. ASVS Assessment

**Target level:** ASVS 5.0 — Level 2

→ [ASVS\_5\_0\_Tracker.xlsx](Deliverables/Sprint_1)
