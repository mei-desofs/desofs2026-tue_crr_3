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
- Low: 1 to 2;
- Medium: 3 to 4;
- High: 6 to 9.

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

- **Tea Stock Levels (`Tea Aggregate: StockLevel`)**  
  Tracks product availability and is directly linked to business operations.  
  Incorrect values may lead to overselling or inconsistencies (violating NFR:02).

- **Relational Database (Infrastructure Layer)**  
  Stores all persistent data, including users, orders, and payments.  
  It is a central asset whose compromise affects the entire system.

- **File System (Tea Image Storage: OS Integration)**  
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

**Attacker Objective:**  
Gain unauthorized access to the system and escalate privileges to perform administrative actions.

**Threat Impact:**  
Unauthorized access may lead to data breaches, reputational damage, and loss of customer trust. It may also allow manipulation of system operations by non-authorized users.


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

**Attacker Objective:**  
Access, modify, or extract sensitive data from the database (users, orders, payments).

**Threat Impact:**  
A successful attack may result in exposure of customer data, legal consequences (e.g., GDPR violations), financial penalties, and loss of business credibility.

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

**Attacker Objective:**  
Exploit race conditions to purchase more items than available or manipulate stock behavior.

**Threat Impact:**  
Incorrect stock management may lead to overselling, order cancellations, customer dissatisfaction, and operational inefficiencies.

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

**Attacker Objective:**  
Gain remote access to the server or execute malicious code.

**Threat Impact:**  
Server compromise may result in full system downtime, data breaches, and significant recovery costs, impacting business continuity.

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

**Attacker Objective:**  
Obtain products or services without completing a legitimate payment.

**Threat Impact:**  
Fraudulent transactions may lead to direct financial loss, accounting inconsistencies, and loss of trust from customers and partners.

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

**Attacker Objective:**  
Collect sensitive information to perform further attacks (e.g., credential reuse, privilege escalation).

**Threat Impact:**  
Leakage of sensitive data may result in regulatory non-compliance (e.g., GDPR), reputational damage, and potential legal actions.

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

**Attacker Objective:**  
Disrupt system availability and degrade service for legitimate users.

**Threat Impact:**  
Service unavailability may result in lost sales, degraded customer experience, and damage to the company’s reputation.



