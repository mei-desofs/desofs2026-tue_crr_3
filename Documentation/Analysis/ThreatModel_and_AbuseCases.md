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
### 4.2 Tampering
### 4.3 Repudiation
### 4.4 Information Disclosure
### 4.5 Denial of Service
### 4.6 Elevation of Privilege

## 5. Threat Model Summary

## 6. Abuse Cases

### 6.1 Abuse Case Approach

### AC-01 – Brute-Force Login Attempt
### AC-02 – Direct Access to Admin or Manager Endpoints
### AC-03 – Manipulation of Stock Through Concurrent Orders
### AC-04 – Malicious Image Upload
### AC-05 – Payment State Forgery
### AC-06 – Sensitive Data Enumeration Through API Errors
### AC-07 – Sales Report Abuse
### AC-08 – API Flooding

## 7. Traceability Between Threats and Abuse Cases
