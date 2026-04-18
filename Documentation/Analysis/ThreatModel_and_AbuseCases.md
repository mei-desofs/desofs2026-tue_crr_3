# Threat Model and Abuse Cases

## 1. Threat Modeling Approach

The threat model for the online tea store backend was developed using the **STRIDE** methodology and was explicitly aligned with the system architecture, domain model, and data flows.

This approach was selected because the Phase 1 deliverable requires not only threat identification, but also clear documentation of system components, external entities, trust boundaries, and relevant data flows. In this project, the threat analysis was therefore performed with direct reference to the system’s DFD-oriented architectural view, rather than only from a purely textual perspective. 

The analysis considered the following project-specific characteristics:

- a REST-based backend application;
- authentication and authorization with multiple roles;
- relational database persistence;
- operating system interactions for image handling;
- interaction with an external payment provider. 

The most security-relevant assets considered in the model are authentication credentials, session tokens, user profile data, order and payment data, stock information, uploaded tea images, and generated sales reports.

---

## 2. DFD-Oriented Threat Model Scope

The threat model is based on the main elements represented in the system architecture and domain diagrams.

### External Entities
- **Client**
- **External Payment Provider**

### Processes
- **AIM Middleware**
- **REST API Controllers**
- **Application Services**
- **Payment API Client**
- **Database Repository**
- **OS File Adapter**

### Data Stores / Persistent Components
- **Relational Database**
- **Server File System Utilities**

### Security-Relevant Domain Areas
- **IAM Aggregate**
- **User Aggregate**
- **Tea Aggregate**
- **Order Aggregate**
- **Payment Aggregate**

The threat analysis focuses on how data moves between these elements and how attackers may exploit those flows.

---

## 3. Trust Boundaries and Data Flows

The following trust boundaries were identified in the system:

### TB1 – External Client to Trusted Backend
This boundary is crossed when the **Client** sends HTTP requests to the backend through the **AIM Middleware** and **REST API Controllers**.

**Main flows crossing this boundary:**
- login and registration requests;
- catalog browsing requests;
- order placement requests;
- payment initiation requests;
- image upload requests;
- report export requests.

This is the primary attack surface of the application.

### TB2 – Backend to Relational Database
This boundary is crossed when **Application Services** interact with the **Database Repository**, which then persists and retrieves data from the **Relational Database**.

**Main flows crossing this boundary:**
- user lookup and persistence;
- stock reads and updates;
- order creation and status update;
- payment persistence;
- sales report data retrieval.

### TB3 – Backend to Server File System
This boundary is crossed when **Application Services** request file operations through the **OS File Adapter**, which interacts with the server operating system and file system utilities.

**Main flows crossing this boundary:**
- image upload;
- image retrieval;
- image deletion.

### TB4 – Backend to External Payment Provider
This boundary is crossed when **Application Services** initiate payment-related operations through the **Payment API Client**, which communicates with the **External Payment Provider**.

**Main flows crossing this boundary:**
- payment initiation;
- payment confirmation or failure status exchange.

### TB5 – Authentication and Authorization Boundary
This boundary separates unauthenticated users from authenticated users and privileged roles. It is crossed after successful authentication and token validation by the **AIM Middleware** and related IAM/session logic.

---

## 4. STRIDE Analysis by DFD Element

### 4.1 External Entity: Client

The **Client** is an external entity and therefore untrusted by default. All incoming requests from this entity must be treated as potentially malicious.

#### Spoofing
An attacker may impersonate a legitimate user by using stolen credentials or replaying session tokens during the flow from **Client -> AIM Middleware -> REST API Controllers**.

#### Tampering
The client may manipulate payloads related to order creation, payment initiation, image upload, or report parameters before they reach backend validation.

#### Repudiation
A malicious user may later deny having placed an order, triggered a payment, or uploaded an image unless those actions are properly logged and attributed.

#### Information Disclosure
The client may probe the API for differences in response bodies, status codes, or timing in order to enumerate accounts, identify internal objects, or infer protected business information.

#### Denial of Service
The client may send a large number of requests to login, registration, catalog, order, report, or upload endpoints, exhausting backend resources.

#### Elevation of Privilege
A normal authenticated customer may attempt to access endpoints intended for `MANAGER` or `ADMIN`, such as inventory management or sales report export.

---

### 4.2 Process: AIM Middleware and REST API Controllers

The **AIM Middleware** and **REST API Controllers** form the main entry point into the trusted backend and are directly exposed to the external client boundary.

#### Spoofing
Weak token validation may allow forged or expired session tokens to be accepted during the authentication and authorization flow.

#### Tampering
Improper request validation may allow attackers to submit malicious payloads that alter business behaviour or later reach downstream components unsafely.

#### Repudiation
If authentication attempts, access denials, and privileged actions are not logged, malicious activity at the API layer may not be attributable.

#### Information Disclosure
Verbose validation errors or distinct authentication failure messages may leak whether a user exists or whether a token is structurally valid.

#### Denial of Service
These components are particularly exposed to flooding attacks because every external request passes through them first.

#### Elevation of Privilege
If middleware or controller-level authorization checks are incomplete, restricted endpoints may become reachable by lower-privileged roles.

---

### 4.3 Process: Application Services

The **Application Services** orchestrate core operations such as authentication, order placement, stock deduction, payment initiation, and report generation.

#### Tampering
This process is highly exposed to business-logic abuse. An attacker may attempt to trigger invalid payment state transitions, manipulate report parameters, or exploit race conditions in stock allocation.

#### Repudiation
Without traceable service-level logging, users may deny having changed order state, initiated report generation, or performed administrative operations.

#### Information Disclosure
Improper DTO mapping or insecure service responses may expose internal fields such as identifiers, payment state, or business-sensitive report data.

#### Denial of Service
Expensive operations such as report generation, payment orchestration, or repeated stock checks can be abused to consume application resources.

#### Elevation of Privilege
Broken authorization enforcement inside services is particularly dangerous because it may bypass controller-level assumptions and expose privileged business actions.

---

### 4.4 Data Store: Relational Database

The **Relational Database** stores users, sessions, teas, stock, orders, payments, and sales-report-related data.

#### Tampering
Improper query construction may allow malicious input to alter or corrupt persistent data, especially through injection-style attacks.

#### Repudiation
If persistent changes are not adequately logged, it may be difficult to determine which authenticated actor triggered a given change.

#### Information Disclosure
Sensitive data stored in the database, including account details and transaction information, may be exposed if access control or query restrictions are insufficient.

#### Denial of Service
A large number of expensive queries or repeated transactional conflicts may reduce database responsiveness and degrade the full application.

This DFD element is particularly relevant to:
- user lookup during authentication;
- stock updates during order placement;
- payment persistence;
- report generation queries.

---

### 4.5 Process / External Interaction: Payment API Client and External Payment Provider

The payment flow crosses a trust boundary between the internal backend and an external service.

#### Spoofing
An attacker may attempt to impersonate payment responses or abuse weak trust assumptions between the backend and the external payment provider.

#### Tampering
Payment requests or returned payment status may be manipulated if integrity checks and trusted workflow validation are insufficient.

#### Repudiation
Users may deny payment initiation or dispute state changes unless payment events are logged with timestamps and references.

#### Information Disclosure
Payment-related metadata may be leaked if error responses or logs expose provider-specific details or transaction references unnecessarily.

#### Denial of Service
Repeated payment initiation attempts may overload the payment integration flow or consume external service quotas.

This part of the DFD is directly linked to `FR-11` and should be explicitly referenced in the report when discussing payment integrity threats. 

---

### 4.6 Process / Data Store: OS File Adapter and File System Utilities

The **OS File Adapter** and the underlying server file system are security-sensitive because they involve interaction with the operating system.

#### Tampering
A malicious actor may attempt to upload files disguised as tea images, manipulate filenames, or abuse delete operations.

#### Information Disclosure
Predictable file paths or weak access restrictions may expose stored image files or other filesystem content.

#### Denial of Service
Very large or repeated upload requests may consume disk space, bandwidth, or file-handling resources.

#### Elevation of Privilege
If file handling is insecure, attackers may attempt to escalate impact from application misuse to server compromise.

This DFD element is directly linked to `FR-07`, which makes it one of the most important trust-boundary crossings in the system.

---

## 5. Threat Model Summary

The DFD-oriented STRIDE analysis shows that the most critical threats are concentrated around the following data flows and trust boundaries:

- **Client -> AIM Middleware / REST API Controllers**, due to spoofing, brute force, information disclosure, and denial of service;
- **Application Services -> Database Repository -> Relational Database**, due to tampering, concurrency issues, and data integrity risks;
- **Application Services -> OS File Adapter -> File System Utilities**, due to malicious upload and file handling abuse;
- **Application Services -> Payment API Client -> External Payment Provider**, due to payment forgery and trust-boundary crossing risks;
- privileged flows associated with `MANAGER` and `ADMIN` operations, due to elevation of privilege.

This makes the threat model more explicitly tied to the system DFDs and architectural flows, improving traceability between system design and threat analysis.

---

## 6. Abuse Cases

### 6.1 Abuse Case Overview

The abuse cases were defined to complement the threat model by describing how malicious actors may misuse legitimate functionality represented in the system architecture and domain model.

Unlike the STRIDE analysis, which is organised by threat category and DFD element, the abuse cases are organised around concrete attacker goals and misuse scenarios. This makes them useful both for documentation and for future security testing activities. 

The following abuse cases were selected because they are directly linked to the most exposed trust boundaries and data flows of the system.

---

### AC-01 – Brute-Force Authentication Attack

**Threat Actor:**  
External attacker

**Target DFD Elements:**  
`Client -> AIM Middleware -> REST API Controllers -> IAM Aggregate / User lookup`

**Related Requirements:**  
FR-02, NFR-03, NFR-08

**Abuse Scenario:**  
The attacker repeatedly submits authentication requests with guessed credentials in an attempt to discover valid accounts or gain unauthorized access.

**Expected Impact:**  
Account compromise, unauthorized access, and potential privilege escalation if administrative users are targeted.

**Related Threat Categories:**  
Spoofing, Denial of Service

**Mitigation Direction:**  
Rate limiting, secure password hashing, generic authentication errors, and robust token/session validation.

---

### AC-02 – Direct Access to Restricted Endpoints

**Threat Actor:**  
Authenticated `CUSTOMER` with malicious intent

**Target DFD Elements:**  
`Client -> REST API Controllers -> Application Services` for restricted administrative functions

**Related Requirements:**  
FR-03, FR-06, FR-10, FR-13

**Abuse Scenario:**  
A normal customer bypasses client-side interface restrictions and sends direct API calls to endpoints intended only for `MANAGER` or `ADMIN`.

**Expected Impact:**  
Unauthorized inventory changes, order status updates, or access to sensitive sales report data.

**Related Threat Categories:**  
Elevation of Privilege

**Mitigation Direction:**  
Strict server-side authorization checks in middleware and service layers.

---

### AC-03 – Concurrent Order Abuse to Break Stock Consistency

**Threat Actor:**  
Authenticated customer

**Target DFD Elements:**  
`Client -> REST API Controllers -> Application Services -> Database Repository -> Relational Database`

**Related Requirements:**  
FR-08, FR-09, NFR-02, NFR-05

**Abuse Scenario:**  
The attacker intentionally sends multiple concurrent order requests for the same tea item when stock is low in order to exploit race conditions.

**Expected Impact:**  
Negative stock values, overselling, and inconsistent order state.

**Related Threat Categories:**  
Tampering

**Mitigation Direction:**  
ACID transactions, locking strategies, and atomic stock update rules.

---

### AC-04 – Malicious File Upload via Tea Image Management

**Threat Actor:**  
Malicious or compromised `MANAGER` / `ADMIN`

**Target DFD Elements:**  
`Client -> REST API Controllers -> Application Services -> OS File Adapter -> File System Utilities`

**Related Requirements:**  
FR-07

**Abuse Scenario:**  
A privileged user uploads a malicious file disguised as a tea image in order to store dangerous content on the server or abuse unsafe file handling logic.

**Expected Impact:**  
Server compromise, malicious file persistence, or later exploitation of the operating system environment.

**Related Threat Categories:**  
Tampering, Elevation of Privilege

**Mitigation Direction:**  
Strict file validation, filename sanitization/randomization, secure storage location, and restrictive filesystem permissions.

---

### AC-05 – Payment State Forgery

**Threat Actor:**  
Authenticated customer or API attacker

**Target DFD Elements:**  
`Client -> REST API Controllers -> Application Services -> Payment API Client -> External Payment Provider`

**Related Requirements:**  
FR-11, NFR-01, NFR-05

**Abuse Scenario:**  
The attacker tampers with the payment initiation or callback flow in an attempt to force an order into a paid state without a legitimate successful payment.

**Expected Impact:**  
Financial fraud, incorrect order confirmation, and loss of transaction integrity.

**Related Threat Categories:**  
Tampering, Spoofing

**Mitigation Direction:**  
Trusted payment workflow validation, strict state transition rules, backend verification, and audit logging.

---

### AC-06 – Sensitive Data Enumeration Through API Errors

**Threat Actor:**  
External attacker

**Target DFD Elements:**  
`Client -> AIM Middleware / REST API Controllers`

**Related Requirements:**  
NFR-03, NFR-04

**Abuse Scenario:**  
The attacker sends malformed, unauthorized, or repeated requests and compares API responses in order to infer valid accounts, internal structure, or protected business information.

**Expected Impact:**  
Disclosure of valid users, identifiers, implementation details, or security-relevant patterns.

**Related Threat Categories:**  
Information Disclosure

**Mitigation Direction:**  
Generic error handling, response normalization, and consistent validation behaviour.

---

### AC-07 – Sales Report Abuse

**Threat Actor:**  
Low-privilege authenticated user

**Target DFD Elements:**  
`Client -> REST API Controllers -> Application Services -> Database Repository`

**Related Requirements:**  
FR-13

**Abuse Scenario:**  
The attacker attempts to generate or access sales reports without proper authorization, or manipulates parameters to retrieve sensitive business information.

**Expected Impact:**  
Disclosure of commercially sensitive data, business metrics, and customer activity patterns.

**Related Threat Categories:**  
Information Disclosure, Elevation of Privilege

**Mitigation Direction:**  
Strict authorization checks and controlled report generation/export rules.

---

### AC-08 – API Flooding

**Threat Actor:**  
External attacker or automated bot

**Target DFD Elements:**  
`Client -> AIM Middleware / REST API Controllers -> Application Services`

**Related Requirements:**  
NFR-06, NFR-07, NFR-08

**Abuse Scenario:**  
The attacker sends a large number of requests to authentication, catalog, order, upload, or report-related endpoints in order to exhaust system resources.

**Expected Impact:**  
Degraded performance, request failures, increased latency, and partial or complete denial of service.

**Related Threat Categories:**  
Denial of Service

**Mitigation Direction:**  
Rate limiting, throttling, timeouts, monitoring, and defensive resource controls.

---

## 6.2 Abuse Case UML Diagrams

To improve the readability of the abuse case analysis, selected abuse cases should also be represented with UML abuse case diagrams. These diagrams make explicit the relationship between normal use cases, malicious use cases, threat actors, and mitigation-oriented behaviours.

The most relevant abuse cases to represent visually are:

- AC-01 – Brute-Force Authentication Attack
- AC-02 – Direct Access to Restricted Endpoints
- AC-04 – Malicious File Upload
- AC-05 – Payment State Forgery

