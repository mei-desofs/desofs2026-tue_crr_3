# Threat Model and Abuse Cases

## 1. Threat Modeling Approach
The threat model for our online tea store backend was developed using the **STRIDE** methodology. This approach was selected because it provides a structured way to identify threats across the main system elements, namely external entities, processes, data flows, and data stores.

The analysis was based on the system requirements, the layered architecture, and the Domain-Driven Design model. Special attention was given to the following security-relevant characteristics of the system:

- authentication and authorization with multiple roles;
- exposure of a REST API;
- relational database persistence;
- operating system interactions for file handling.

The main assets considered in this threat model include user credentials, authentication tokens, order and payment data, tea stock levels, uploaded images, and API endpoints. These assets were mapped against relevant trust boundaries and attack surfaces in order to identify realistic threats.

---

## 2. System Scope and Security-Relevant Components
The threat model covers the backend application of the online tea store, including the following major components:

- **Client Applications / API Consumers**  
  External actors interacting with the system through HTTP requests.

- **REST API (Presentation Layer)**  
  Entry point for registration, authentication, catalog access, order placement, payment processing, inventory management, and report export.

- **Application Layer / Services**  
  Contains the orchestration logic for use cases such as placing orders, updating stock, processing payments, and enforcing role-based access.

- **Domain Layer**  
  Contains the main aggregates: `User`, `Tea`, and `Order`, as well as related domain entities and value objects.

- **Relational Database**  
  Persistent storage for users, teas, orders, order items, payment state, and sales reports.

- **File System / OS Integration**  
  Used for upload, storage, retrieval, and deletion of tea images.

---

## 3. Trust Boundaries
The following trust boundaries were identified:

1. **External Client -> REST API**  
   Requests originating from external clients cross into the trusted backend environment.

2. **REST API / Application Layer -> Database**  
   Application logic interacts with persistent storage and must ensure data integrity and query safety.

3. **REST API / Application Layer -> File System**  
   Image upload and deletion operations cross into the operating system environment.

4. **Authentication Boundary**  
   Unauthenticated users become authenticated users after successful credential validation and token issuance.

These trust boundaries are important because they represent the main locations where malicious input, privilege abuse, tampering, and information disclosure attempts may occur.

---

## 4. STRIDE Analysis

### 4.1 Spoofing
**Threat:** Account impersonation through stolen credentials or forged authentication tokens.

**Affected Components:**  
`User Aggregate`, authentication endpoints, JWT/session token handling.

**Attack Vector:**  
An attacker may attempt credential stuffing, brute-force authentication, token replay, or exploitation of weak session validation.

**Impact:**  
Unauthorized access to user accounts or privileged roles such as `MANAGER` or `ADMIN`.

**Related Requirements:**  
FR-02, FR-03, NFR-03, NFR-08

**Mitigation Direction:**  
Strong password hashing, secure token validation, token expiration, role verification, and rate limiting on sensitive endpoints.

---

### 4.2 Tampering
**Threat:** Unauthorized modification of system data or state.

**Affected Components:**  
`Order Aggregate`, `Tea Aggregate`, payment processing, relational database, file storage.

**Attack Vector:**  
An attacker may manipulate request payloads, attempt SQL injection, alter payment status transitions, tamper with stock updates, or upload malicious files disguised as tea images.

**Impact:**  
Corruption of stock levels, fraudulent payment completion, compromised order integrity, or malicious files stored on the server.

**Related Requirements:**  
FR-07, FR-09, FR-11, NFR-01, NFR-02, NFR-05

**Mitigation Direction:**  
Prepared statements, strict backend validation, transactional controls, secure state transition validation, file type verification, and safe storage practices.

---

### 4.3 Repudiation
**Threat:** Users or administrators deny having performed a given action.

**Affected Components:**  
Authentication flows, order updates, payment processing, inventory management, and report export.

**Attack Vector:**  
A malicious actor may deny having changed an order status, uploaded a file, processed a payment, or generated a report if no auditable logging exists.

**Impact:**  
Loss of traceability, difficulty in incident investigation, and inability to attribute actions to specific users.

**Related Requirements:**  
FR-10, FR-11, FR-13, NFR-09

**Mitigation Direction:**  
Audit logging, timestamped records, traceability of privileged actions, and secure retention of logs.

---

### 4.4 Information Disclosure
**Threat:** Exposure of confidential or internal data.

**Affected Components:**  
REST API responses, authentication flow, database queries, report export, and file access.

**Attack Vector:**  
Sensitive fields may be leaked through verbose error messages, insecure DTO mapping, broken access control, predictable file paths, or insufficient protection of exported reports.

**Impact:**  
Disclosure of password hashes, customer details, internal identifiers, order data, or business-sensitive sales information.

**Related Requirements:**  
FR-04, FR-13, NFR-03, NFR-04

**Mitigation Direction:**  
Least-privilege access, response filtering, generic error handling, strict authorization checks, and access-controlled exports.

---

### 4.5 Denial of Service
**Threat:** System resources become unavailable or degraded.

**Affected Components:**  
REST API, authentication endpoints, payment processing endpoints, image upload functionality, and database connectivity.

**Attack Vector:**  
An attacker may flood login, registration, catalog, or order endpoints, upload oversized files, or trigger repeated expensive operations.

**Impact:**  
Reduced availability, slower response times, failed transactions, and degraded user experience.

**Related Requirements:**  
NFR-06, NFR-07, NFR-08

**Mitigation Direction:**  
Rate limiting, request throttling, timeouts, input size restrictions, graceful fault handling, and operational monitoring.

---

### 4.6 Elevation of Privilege
**Threat:** A lower-privileged user gains access to higher-privileged functionality.

**Affected Components:**  
Role enforcement in API endpoints, inventory management, order status updates, report export, and file operations.

**Attack Vector:**  
A `CUSTOMER` may attempt direct requests to endpoints intended for `MANAGER` or `ADMIN`, or exploit broken authorization checks in the application layer.

**Impact:**  
Unauthorized inventory changes, report access, file manipulation, or order state modification.

**Related Requirements:**  
FR-03, FR-06, FR-07, FR-10, FR-13

**Mitigation Direction:**  
Strict server-side authorization, role verification on every protected endpoint, and defense-in-depth validation inside services.

---

## 5. Threat Model Summary
The STRIDE analysis shows that the highest-risk areas of the system are:

- authentication and session handling;
- role-based access control;
- order and stock consistency;
- payment processing integrity;
- file upload and file system interaction;
- sensitive data exposure through API responses and report exports.

These threats are consistent with the architecture and requirements of the system, particularly because the platform combines authentication, financial operations, file handling, and privileged administrative actions in a REST-based backend.

---

## 6. Abuse Cases

### 6.1 Abuse Case Approach
Abuse cases were defined to complement the threat model by describing how malicious actors may intentionally misuse legitimate system functionality.

Each abuse case includes:

- the threat actor;
- the targeted functionality;
- the misuse scenario;
- the expected impact;
- the related threat category;
- the mitigation direction.

---

### AC-01 – Brute-Force Login Attempt
**Threat Actor:**  
External attacker

**Target Functionality:**  
User authentication

**Related Requirements:**  
FR-02, NFR-08

**Abuse Scenario:**  
The attacker repeatedly submits login attempts against valid or guessed email addresses in order to discover valid credentials.

**Expected Impact:**  
Account compromise, unauthorized access, and possible privilege escalation if privileged accounts are targeted.

**Related Threat Category:**  
Spoofing, Denial of Service

**Mitigation Direction:**  
Rate limiting, secure password storage, anomaly detection, and strong authentication controls.

---

### AC-02 – Direct Access to Admin or Manager Endpoints
**Threat Actor:**  
Authenticated customer with malicious intent

**Target Functionality:**  
Inventory management, order status updates, and sales report export

**Related Requirements:**  
FR-06, FR-10, FR-13

**Abuse Scenario:**  
A regular customer sends crafted API requests directly to endpoints intended for `MANAGER` or `ADMIN`, attempting to bypass client-side restrictions.

**Expected Impact:**  
Unauthorized modification of teas, order states, or access to sensitive business reports.

**Related Threat Category:**  
Elevation of Privilege

**Mitigation Direction:**  
Strict server-side RBAC enforcement and authorization checks on every restricted operation.

---

### AC-03 – Manipulation of Stock Through Concurrent Orders
**Threat Actor:**  
Malicious or opportunistic authenticated customer

**Target Functionality:**  
Order placement and stock allocation

**Related Requirements:**  
FR-08, FR-09, NFR-02, NFR-05

**Abuse Scenario:**  
The attacker sends multiple concurrent order requests for the same tea item when stock is low, attempting to force inconsistent stock updates.

**Expected Impact:**  
Negative stock values, overselling, and business logic inconsistency.

**Related Threat Category:**  
Tampering

**Mitigation Direction:**  
Transactional enforcement, locking mechanisms, and atomic stock validation and update operations.

---

### AC-04 – Malicious Image Upload
**Threat Actor:**  
Compromised or malicious `MANAGER`/`ADMIN`

**Target Functionality:**  
Tea image management

**Related Requirements:**  
FR-07

**Abuse Scenario:**  
A privileged user uploads a malicious file disguised as an image in an attempt to store executable or dangerous content on the server.

**Expected Impact:**  
Server compromise, malicious file storage, or later exploitation through insecure file handling.

**Related Threat Category:**  
Tampering, Elevation of Privilege

**Mitigation Direction:**  
Strict file type validation, safe storage location, filename randomization, and permission hardening.

---

### AC-05 – Payment State Forgery
**Threat Actor:**  
Authenticated customer or API attacker

**Target Functionality:**  
Payment processing

**Related Requirements:**  
FR-11, NFR-01, NFR-05

**Abuse Scenario:**  
The attacker tampers with the client request or API payload in an attempt to force the payment status to `COMPLETED` without a legitimate successful payment.

**Expected Impact:**  
Financial loss, fraudulent order confirmation, and compromised transaction integrity.

**Related Threat Category:**  
Tampering

**Mitigation Direction:**  
Server-side validation of payment state transitions, trusted payment workflow enforcement, and audit logging.

---

### AC-06 – Sensitive Data Enumeration Through API Errors
**Threat Actor:**  
External attacker

**Target Functionality:**  
Authentication, registration, order access, and general API interaction

**Related Requirements:**  
NFR-03, NFR-04

**Abuse Scenario:**  
The attacker intentionally sends malformed or unauthorized requests to observe differences in API responses, error messages, or timing behaviour.

**Expected Impact:**  
Disclosure of valid accounts, internal identifiers, application structure, or sensitive implementation details.

**Related Threat Category:**  
Information Disclosure

**Mitigation Direction:**  
Standardized generic error handling, controlled response content, and consistent validation behaviour.

---

### AC-07 – Sales Report Abuse
**Threat Actor:**  
Unauthorized or low-privilege authenticated user

**Target Functionality:**  
Sales report export

**Related Requirements:**  
FR-13

**Abuse Scenario:**  
An attacker attempts to access or generate sales reports containing business-sensitive data by manipulating API requests or report parameters.

**Expected Impact:**  
Disclosure of commercially sensitive information, customer activity patterns, and business metrics.

**Related Threat Category:**  
Information Disclosure, Elevation of Privilege

**Mitigation Direction:**  
Authorization checks, strict validation of report generation permissions, and access-controlled export mechanisms.

---

### AC-08 – API Flooding
**Threat Actor:**  
External attacker or bot

**Target Functionality:**  
Login, registration, catalog browsing, order placement, and upload endpoints

**Related Requirements:**  
NFR-06, NFR-07, NFR-08

**Abuse Scenario:**  
The attacker sends a large volume of requests in order to exhaust API, database, or file-handling resources.

**Expected Impact:**  
Service degradation, request failures, increased latency, and possible denial of service.

**Related Threat Category:**  
Denial of Service

**Mitigation Direction:**  
Rate limiting, throttling, timeout controls, and operational monitoring.

---


## 7. Traceability Between Threats and Abuse Cases
The abuse cases are directly derived from the identified STRIDE threats and from the most security-sensitive requirements of the system.

In particular:

- authentication abuse cases support spoofing analysis;
- role bypass abuse cases support elevation of privilege analysis;
- stock and payment manipulation abuse cases support tampering analysis;
- report and error-based enumeration abuse cases support information disclosure analysis;
- flooding abuse cases support denial of service analysis.

This traceability strengthens the consistency of the Phase 1 deliverable and supports later security testing activities.
