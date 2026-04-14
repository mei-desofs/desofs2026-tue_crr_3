# 1. Risk assessment and Mitigations
## 1.1 Risk Assessment and Methodology
A qualitative risk assessment methodology was adopted based on the following formula: **Risk = Likelihood x Impact**.
Each factor is evaluated on a three:level sclale
| Level | Likelihood | Impact |
|-------|------------|--------|
| Low   | 1          | 1      |
| Medium| 2          | 2      |
| High  | 3          | 3      |

Risk levels are classified as: 
*Low: 1 to 2;
*Medium: 3 to 4;
*High: 6 to 9.

This methodology allows prioritization of threats affecting critical system assets such as user credentials, finacncial transactions, stock management and file system operations.

## 1.2 Critical Assets

Based on the defined Domain Model (DDD) and layered architecture, the following assets were identified as critical to the system’s security and operation:

- **User Credentials (`User Aggregate`)**  
  Includes email, passwordHash, and role (CUSTOMER, MANAGER, ADMIN).  
  This asset is critical as it controls authentication and authorization across the system.

- **Order Data (`Order Aggregate`)**  
  Contains information about user purchases, quantities, and order status.  
  Compromise of this data may lead to financial inconsistencies and loss of integrity.

- **Payment Information (`Payment Aggregate`)**  
  Represents payment status and transaction results.  
  This asset is highly sensitive as manipulation may result in financial fraud.

- **Tea Stock Levels (`Tea Aggregate – StockLevel`)**  
  Tracks product availability and is directly linked to business operations.  
  Incorrect values may lead to overselling or inconsistencies (violating NFR:02).

- **Relational Database (Infrastructure Layer)**  
  Stores all persistent data, including users, orders, and payments.  
  It is a central asset whose compromise affects the entire system.

- **File System (Tea Image Storage – OS Integration)**  
  Used to store uploaded tea images (FR07).  
  Improper handling may lead to malicious file execution or server compromise.

- **REST API Endpoints (Presentation Layer)**  
  Entry point for all client interactions with the system.  
  Vulnerabilities here may expose all underlying components.

- **Authentication Tokens (JWT)**  
  Used to maintain user sessions and authorize requests.  
  If compromised, attackers may impersonate legitimate users.

## 1.3 Identified Risks

### R1 – Authentication Bypass

**Affected Components:**  
`User Aggregate`, `REST API (Presentation Layer)`

**Related Requirements:**  
FR02 (Authentication), NFR03 (Secure Error Handling)

**Description:**  
Improper authentication mechanisms or weak token validation may allow unauthorized users to gain access to privileged roles such as ADMIN or MANAGER.

**Threat Impact:**  
Attackers may access sensitive data and perform unauthorized operations, compromising both confidentiality and integrity of the system.

**Likelihood:** High (3)  
**Impact:** High (3)  
**Risk Level:** High (9)

---

### R2 SQL Injection

**Affected Components:**  
`Infrastructure Layer (Database Repository)`, `Application Services`

**Related Requirements:**  
NFR01 (Input Validation), NFR05 (Data Integrity)

**Description:**  
Improper validation or sanitization of user input in API requests may allow attackers to inject malicious SQL queries.

**Threat Impact:**  
This may result in unauthorized access, modification, or deletion of data in users, orders, and payments.

**Likelihood:** High (3)  
**Impact:** High (3)  
**Risk Level:** High (9)

---

### R3 Concurrency Issues (Race Condition in Stock Allocation)

**Affected Components:**  
`Order Aggregate`, `Tea Aggregate (StockLevel)`

**Related Requirements:**  
FR09 (Stock Allocation), NFR02 (Concurrency Handling)

**Description:**  
Simultaneous purchase requests for the same product may lead to inconsistent stock values if concurrency is not properly handled.

**Threat Impact:**  
This may result in negative stock levels, overselling, and data inconsistency.

**Likelihood:** Medium (2)  
**Impact:** High (3)  
**Risk Level:** High (6)

---

### R4 Malicious File Upload

**Affected Components:**  
`Tea Aggregate (uploadImage)`, `OS File Adapter (Infrastructure Layer)`

**Related Requirements:**  
FR07 (OS File Operations), NFR01 (Input Validation)

**Description:**  
Unrestricted file upload functionality may allow attackers to upload malicious or executable files to the server.

**Threat Impact:**  
This may lead to server compromise, execution of malicious code, or data breaches.

**Likelihood:** High (3)  
**Impact:** High (3)  
**Risk Level:** High (9)

---

### R5 Payment Status Manipulation

**Affected Components:**  
`Payment Aggregate`

**Related Requirements:**  
FR10 (Payment Processing), NFR05 (Data Integrity)

**Description:**  
An attacker may attempt to manipulate the payment status, for example by forcing a transaction to be marked as COMPLETED.

**Threat Impact:**  
This may lead to financial fraud, incorrect order processing, and revenue loss.

**Likelihood:** Medium (2)  
**Impact:** High (3)  
**Risk Level:** Medium–High (6)

---

### R6 Exposure of Sensitive Data

**Affected Components:**  
`REST API Responses`, `User Aggregate`

**Related Requirements:**  
NFR03 (Secure Error Handling), NFR01 (Data Protection)

**Description:**  
Improper handling of API responses may expose sensitive information such as passwordHash or internal system data.

**Threat Impact:**  
This may result in credential compromise and violation of data confidentiality.

**Likelihood:** Medium (2)  
**Impact:** High (3)  
**Risk Level:** Medium–High (6)

---

### R7 Denial of Service (DoS)

**Affected Components:**  
`REST API (Presentation Layer)`

**Related Requirements:**  
NFR08 (Rate Limiting)

**Description:**  
Excessive or malicious requests may overload the system, leading to reduced performance or service unavailability.

**Threat Impact:**  
This affects system availability and user experience.

**Likelihood:** Medium (2)  
**Impact:** Medium (2)  
**Risk Level:** Medium (4)

---

## 1.4 Mitigation Strategies

### R1 Authentication Bypass

**Mitigation Measures:**

- Use secure password hashing (`bcrypt`)
- Implement JWT authentication with expiration time
- Enforce Role-Based Access Control (RBAC) in the Application Layer
- Apply rate limiting on login endpoints (NFR-08)
- Use generic error messages to avoid information leakage (NFR-03)

**Justification:**  
These measures ensure that authentication is secure and prevent unauthorized access to privileged roles.

---

### R2 SQL Injection

**Mitigation Measures:**

- Use prepared statements or ORM (e.g., JPA/Hibernate)
- Validate and sanitize all user inputs at the Application Layer
- Avoid dynamic query construction
- Apply the principle of least privilege in database access
- Monitor and log suspicious database queries

**Justification:**  
These controls prevent malicious input from being interpreted as executable SQL, protecting data integrity and confidentiality.

---

### R3 Concurrency Issues (Race Condition in Stock Allocation)

**Mitigation Measures:**

- Use ACID-compliant database transactions (NFR05)
- Implement locking mechanisms (optimistic or pessimistic locking)
- Validate stock availability before confirming orders
- Ensure atomic updates to stock levels

**Justification:**  
These strategies guarantee consistency of stock data and prevent overselling due to concurrent operations.

---

### R4 Malicious File Upload

**Mitigation Measures:**

- Validate file type using MIME type and extension
- Restrict allowed file formats (e.g., only images)
- Limit file size
- Rename uploaded files to avoid path manipulation
- Store files outside the public web directory
- Apply strict file system permissions

**Justification:**  
These controls prevent execution of malicious files and protect the server from compromise.

---

### R5 Payment Status Manipulation

**Mitigation Measures:**

- Ensure payment status is controlled exclusively by backend logic
- Validate all payment state transitions
- Implement idempotent operations in payment processing
- Maintain audit logs of all payment actions

**Justification:**  
These mechanisms prevent unauthorized manipulation of financial transactions and ensure traceability.

---

### R6 Exposure of Sensitive Data

**Mitigation Measures:**

- Use DTOs to control API responses
- Exclude sensitive fields such as `passwordHash`
- Standardize secure error responses (NFR-03)
- Avoid logging sensitive information

**Justification:**  
These measures protect user data confidentiality and prevent information leakage.

---

### R7 Denial of Service (DoS)

**Mitigation Measures:**

- Implement rate limiting (NFR-08)
- Configure request timeouts
- Limit payload sizes
- Monitor traffic and generate alerts for abnormal activity

**Justification:**  
These controls reduce the impact of excessive requests and maintain system availability.

---
