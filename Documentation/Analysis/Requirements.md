# Requirements Specification

## 1. Functional Requirements

### 1.1 User Aggregate
* **FR-01 (Registration):** The system must allow users to register an account by providing an email address and password.
* **FR-02 (Authentication):** The system must allow users to authenticate securely and obtain a valid session token.
* **FR-03 (Role Management):** The system must assign one of three distinct roles to users: `CUSTOMER`, `MANAGER`, or `ADMIN`.
* **FR-04 (Address Management):** The system must allow authenticated users to view and update their shipping addresses.

### 1.2 Tea Aggregate 
* **FR-05 (View Catalog):** The system must allow all users to browse available Teas, viewing their names and prices.
* **FR-06 (Manage Inventory):** The system must allow `MANAGER` and `ADMIN` users to create, update, and delete Tea entries.
* **FR-07 (Image Management - OS Integration):** The system must allow `MANAGER` and `ADMIN` users to upload, read, and delete Tea images from the backend server`s file system .

### 1.3 Order Aggregate
* **FR-08 (Place Order):** The system must allow authenticated `CUSTOMER` to place an order containing specific quantities of Teas.
* **FR-09 (Stock Allocation):** The system must automatically deduct the purchased quantities from the Tea's `StockLevel` when an order is successfully placed.
* **FR-10 (Order Status):** The system must allow `MANAGER's` to update the  status of an order (e.g., from `PENDING` to `SHIPPED`).
* **FR-11 (Payment Processing):** The system must allow `CUSTOMERS` to proceed with payment and update the payment status (e.g., `COMPLETED` or `FAILED`).
* **FR-13 (Sales Report Export ):** The system must allow `ADMINS`  to input a custom "Report Name" and a date range to request a sales report. 

## 2. Non-Functional Requirements (FURPS+ Model)

### 2.1 Functionality (Security & Compliance)
* **NFR-01 (Data Accuracy):** The system must calculate all financial transactions  with absolute precision avoiding floating point errors and other inaccuracies.
* **NFR-02 (Concurrency Handling):** The system must handle concurrent modifications safely (e.g., handling scenarios where two users attempt to buy the last unit of a specific Tea simultaneously) to prevent negative `StockLevel` states.

### 2.2 Usability
* **NFR-03 (Secure Error Handling):** The REST API must return standardized, generic error messages to clients (e.g., "Invalid credentials").
* **NFR-04 (API Documentation):** The backend must provide comprehensive API documentation  to facilitate frontend integration and structured security testing.

### 2.3 Reliability
* **NFR-05 (Transactional Integrity):** Operations that span multiple domain entities must be encapsulated within strictly enforced ACID database transactions.
* **NFR-06 (Fault Tolerance):** The application must be capable of handling database connection timeouts gracefully without crashing the main process.

### 2.4 Performance
* **NFR-7 (Response Times):** The system should process standard API requests (reads) in under 500ms, and complex operations (file uploads, payment processing) in under 3 seconds.
* **NFR-8 (Rate Limiting):** The system must implement rate-limiting on sensitive endpoints (such as login and registration) to a maximum of 10 requests per minute per IP address.

### 2.5 Supportability
* **NFR-9 (Testability):** The architecture must support automated integration into a DevSecOps pipeline.

### 2.6 Plus (+) Constraints
* **NFR-10 (Database Constraint):** The system must persist data using a relational database engine.
* **NFR-11 (Architectural Style):** The backend application must be implemented as a web server exposing a RESTful Application Programming Interface.

## 3. Validation and Business Logic Requirements

### BR-01 Stock Management
- Stock values shall never become negative.
- Only users with MANAGER or ADMIN roles may modify inventory levels.
- All stock modifications shall be validated before persistence.

### BR-02 Order Creation
- Orders shall only be created when sufficient stock exists.
- Product prices shall always be obtained from the database and never from client-supplied values.
- Stock allocation shall occur atomically with order creation.

### BR-03 Order Access
- Customers shall only be able to access their own order history.
- Order ownership shall be validated using the authenticated user identity.

### BR-04 Order Cancellation
- Only the owner of an order may cancel it.
- Only orders in the PENDING state may be cancelled.

### BR-05 Transaction Integrity
- Inventory updates and order creation shall be performed as a single business transaction.
- Concurrent operations shall not result in inconsistent stock levels.

### BR-06 Financial Integrity
- Financial calculations shall use precise decimal arithmetic.
- Client-supplied monetary values shall not be trusted.