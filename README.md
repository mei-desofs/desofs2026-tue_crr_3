# Phase 2 – Sprint 1 Deliverable

**Project:** Tea Shop API   
**SonarCloud :** [mei-desofs-1_tue-crr-3](https://sonarcloud.io/project/overview?id=mei-desofs-1_tue-crr-3)

---

## 1. Development

### 1.1 Implemented Functionality

The following aggregates and their primary endpoints were implemented this sprint:

| Aggregate         | Key Endpoints                                                                                              | Roles Permitted   |
|-------------------|------------------------------------------------------------------------------------------------------------|-------------------|
| **User**          | `POST /api/auth/signUp`, `POST /api/auth/login`, `POST /api/auth/logout`, `POST /api/auth/change-password` | ALL               |
| **IAM**           | Internal token and session lifecycle                                                                       | Internal          |
| **Catalog (Tea)** | `GET /api/catalog`, `GET /api/catalog/{id}`, `PATCH /api/catalog/{id}/stock`                               | ALL               |
| **Category**      | `POST /api/categories`, `PATCH /api/categories/{id}`, `DELETE /api/categories/{id}`                        | ADMIN             |
| **Order**         | `POST /api/orders`, `GET /api/orders/me`                                                                   | CUSTOMER, MANAGER |
Authorization enforces three distinct roles (`CUSTOMER`, `MANAGER`, `ADMIN`) .

---

### 1.2 Security Practices Adopted

The following security controls were deliberately implemented during development. Each is shown by a code reference.

| Practice                                                                                   | Evidence                                                                                                                                              |
|--------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------|
| Parameterized queries via EF Core — no raw SQL with user input                             | [UserRepository.cs](TeaShop/TeaShop/Infrastructure/Persistence/Repositories/UserRepository.cs)                                                        |
| Session tokens generated with `RandomNumberGenerator` (256-bit entropy)                    | [SessionToken.cs](TeaShop/TeaShop/Domain/IAM/SessionToken.cs)                                                                                         |
| Role-based authorization enforced at the controller layer via `[Authorize(Roles = "...")]` | [UserController.cs](TeaShop/TeaShop/Presentation/UserController.cs)                                                                                   |
| No secrets in `appsettings.json` —  DB connection string loaded from secrets               | [TeaShop.csproj](https://github.com/mei-desofs/desofs2026-tue_crr_3/blob/bd0da8b2e3eabf4a6501a422e40508aed1d5dfb9/TeaShop/TeaShop/TeaShop.csproj#L10) |
| Generic error responses in production — no stack traces exposed to clients                 | [GenericExceptionMiddleware.cs ](TeaShop/TeaShop/Infrastructure/Middleware/GenericExceptionMiddleware.cs)                                             |
| Domain value objects enforce invariants — prevents type confusion and invalid state        | [Email.cs](TeaShop/TeaShop/Domain/Users/Email.cs)                                                                                                     |
---

### 1.3 Code Reviews

All code changes were merged to the `main` branch via Pull Requests. Each merge required:

*   **Automated Validation:** Successful completion of the CI/CD pipeline, which executes build, test, and security-focused linting checks.
*   **Mandatory Review:** Explicit manual review of the PR by a team member.
---

### 1.4 Static Analysis (SAST)

Two SAST tools run automatically on every push and pull request.

**SonarCloud** is integrated in the CI pipeline (`.github/workflows/ci.yml`). 

- Pipeline job: `build → Sonar Begin → build → Test with Coverage → Sonar End`
- SonarCloud dashboard: [mei-desofs-1_tue-crr-3 on SonarCloud](https://sonarcloud.io/project/overview?id=mei-desofs-1_tue-crr-3)

[Successful pipeline example run](https://github.com/mei-desofs/desofs2026-tue_crr_3/actions/runs/25952080145/job/76291798673)
 
**CodeQL** runs automatically via GitHub Advanced Security on every push to `main` and every pull request targeting `main`.

[Code Scanning ](https://github.com/mei-desofs/desofs2026-tue_crr_3/security/code-scanning)

---

### 1.5 Software Composition Analysis (SCA)

Dependency security is enforced at the pull request level. The **GitHub Dependency Review Action** runs on every PR targeting `main` (`.github/workflows/security.yml`, job `dependency-review`) and is configured to **fail the pipeline on any dependency with HIGH or CRITICAL severity CVEs**, blocking the merge.

# from security.yml
```yaml
- uses: actions/dependency-review-action@v4
  with:
    fail-on-severity: high
```

This directly mitigates supply chain risks against third-party libraries.


---

## 2. Build and Test

### 2.1 Component Inventory (SBOM)

A Software Bill of Materials is generated automatically on every push and pull request by the `sbom` job in `.github/workflows/security.yml` using the **CycloneDX** tool (`dotnet-CycloneDX`). The output is a JSON SBOM file uploaded as a pipeline artifact named `sbom`.


[Example of pipeline run with sbom downloadable ](https://github.com/mei-desofs/desofs2026-tue_crr_3/actions/runs/25990721159)

---

### 2.2 Test Execution

Tests run as part of the CI pipeline (`build` job, `.github/workflows/ci.yml`). Coverage is collected with `dotnet-coverage` and reported to SonarCloud.

[Test Project](TeaShop/TeaShop.Test)

---

### 2.3 Dynamic Analysis (DAST)

**OWASP ZAP API Scan** runs automatically on every push to `main` (`.github/workflows/security.yml`, job `dynamic_analysis`). It:

1. Spins up the API with `dotnet run` in `CI` environment mode on port `8080`
2. Waits for the application to be ready (polling `http://localhost:8080`)
3. Runs `zaproxy/action-api-scan@v0.9.0` against the OpenAPI spec at `/openapi/v1.json`
4. Uploads the ZAP report as the `zap-api-report` artifact

The scan is configured with `fail_action: false` so it does not block the pipeline, but all findings are reviewed. 

[example pipeline run with report downloadable](https://github.com/mei-desofs/desofs2026-tue_crr_3/actions/runs/25990721159)


### 2.4 Configuration Validation

| Check                                                          | Evidence                                             |
|----------------------------------------------------------------|------------------------------------------------------|
| No secrets committed to `appsettings.json`                     | [appsettings.json](TeaShop/TeaShop/appsettings.json) |
| Different settings for development and production environments | [Program.cs](TeaShop/TeaShop/Program.cs)             |

---

## 3. Pipeline Automation

Both pipelines run automatically — no manual steps are required to trigger security checks.

| Stage                                | Tool                              | Trigger               | Pipeline File  | Automated |
|--------------------------------------|-----------------------------------|-----------------------|----------------|-----------|
| Build & Unit/Integration Tests       | `dotnet build` + `dotnet test`    | Push & PR to `main`   |  `ci.yml`      |  ✅        |
| Code Coverage                        | `dotnet-coverage` → SonarCloud    | Push & PR to `main`   | `ci.yml`       | ✅         |
| SAST — Semantic analysis             | SonarCloud                        | Push & PR to `main`   | `ci.yml`       | ✅         |
| SAST — Deep injection/path analysis  | CodeQL (GitHub Advanced Security) | Push & PR to `main`   | GitHub auto    | ✅         |
| SCA — Dependency vulnerability check | Dependency Review                 | PR to `main` only     | `security.yml` | ✅         |
| Component Inventory (SBOM)           | CycloneDX                         | Push & PR to `main`   | `security.yml` | ✅         |
| DAST — Active API scan               | OWASP ZAP (`action-api-scan`)     | Push to `main` only   | `security.yml` | ✅         |
| Scheduled full security scan         | All security jobs                 | Weekly (Mon 03:00)    | `security.yml` | ✅         |

→ [CI.yml](.github/workflows/ci.yml) · [security](.github/workflows/security.yml)

---

## 4. ASVS Assessment

**Target level:** ASVS 5.0 — Level 2

→ [ASVS\_5\_0\_Tracker.xlsx](Deliverables/Sprint_1)

---
