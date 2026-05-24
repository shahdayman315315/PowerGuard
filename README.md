# PowerGuard EMS (Energy Management System) ⚡

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-blue.svg)]()
[![CQRS](https://img.shields.io/badge/Pattern-CQRS_%2F_MediatR-green.svg)]()
[![Azure Deployed](https://img.shields.io/badge/Deployment-Azure_App_Service-orange.svg)]()

PowerGuard is an Enterprise-grade Industrial Energy Management Web API engineered to log, track, and evaluate power consumption across large factories and complex organizations. Built on **.NET 8** following **Clean Architecture**, **CQRS**, and **Domain-Driven Design (DDD)** principles, the platform acts as the centralized system to eliminate energy waste, reduce electricity costs, and provide management with accurate, real-time insights into departmental and machine-level consumption.

---

## 🚀 Project Overview

### ❌ The Problem
Large industrial facilities and multi-department organizations face exorbitant electricity costs due to inefficient energy usage and a critical lack of real-time monitoring. Currently, facility managers operate blindly, with no clear visibility into which specific departments, production lines, or individual machines consume the most power. This structural gap leads to undetected energy waste, operational bottlenecks, and unnecessary expenses.

### ✔️ The Solution
**PowerGuard** provides a smart, data-driven system designed to monitor and analyze electricity consumption accurately. By replacing manual audits with structured, real-time backend logging, the system maps the entire enterprise infrastructure. It allows management to isolate waste points, control high-consumption zones, benchmark resource allocation, and instantly receive critical over-consumption alerts.

---

## ✨ System Features

* **Role-Specific Professional Dashboards:** Tailored API metrics aggregation separating high-level data views into distinct operational dashboards:
  * **Admin Dashboard:** System-wide visibility, user management control, global threshold configurations, and full enterprise logs.
  * **Factory Manager Dashboard:** Operational overview of specific factory zones, historical efficiency charting, and cost allocation summaries.
  * **Department Manager & Factory Manager Dashboards:** Granular localized consumption data tracking, live machine-level updates,Departments Consumption comparison , and instant threshold warnings.
* **Hierarchical Infrastructure Mapping:** Comprehensive backend model supporting structured organization trees (Corporate -> Individual Factories -> Production Zones -> Departments -> Specific Machinery).
* **Data Logging & Meter Auditing API:** Optimized RESTful endpoints for recording precise electrical metrics (Voltage, Current, Kilowatt-hour) uploaded securely by administrators or authorized field systems.
* **Real-Time Threshold Alerting:** Instantaneous push-notification framework driven by **SignalR Hubs**, broadcasting immediate warnings to active clients when a department's logged data breaches its safety boundaries.
* **Dynamic Efficiency Evaluator Engine:** Extensible computation layer implementing the **Strategy Design Pattern** to analyze consumption figures dynamically against operational hours, production outputs, or asset classifications.
* **Enterprise Identity & RBAC:** Secure user management and granular Role-Based Access Control enforcing strict separation of duties among Administrators, Factory Managers, and Maintenance Engineers.

---

## 🛠️ Tech Stack & Tooling

### Core Frameworks
* **Backend Runtime:** .NET 8 (ASP.NET Core Web API)
* **Database & ORM:** Microsoft SQL Server / Azure SQL DB | Entity Framework Core (Code-First approach)
* **Asynchronous Mediation:** MediatR (In-Process Memory Bus)
* **Real-Time Middleware:** ASP.NET Core SignalR (WebSockets fallback)

### Security & Architecture Components
* **Authentication:** ASP.NET Core Identity Framework
* **Token Protocol:** JSON Web Tokens (JWT) with secure Refresh Token Rotation mechanisms
* **Validation:** FluentValidation for incoming command/query body payloads

### Cloud & DevOps Engineering
* **Cloud Infrastructure:** Microsoft Azure (App Services for API hosting, Azure SQL for managed database)
* **CI/CD Pipeline:** GitHub Actions automating code build and direct deployment artifacts to Azure slots.
* **Development Utilities:** Postman for API testing, Git for version control.

---

## 🏗️ Backend Architecture

The backend enforces a strict **Separation of Concerns (SoC)** by strictly adhering to **Clean Architecture** patterns. This ensures the application core remains pure, highly testable, and isolated from UI frameworks or persistence layer breaking changes.

### Architectural Layers Mapping

| Layer Name | Project Namespace | Primary Responsibilities & Components |
| :--- | :--- | :--- |
| **Presentation** | `PowerGuard.WebAPI` | API Controllers, SignalR Hub Infrastructure, App Middleware, AppSettings |
| **Infrastructure** | `PowerGuard.Infrastructure` | EF Core Data Context, Migrations, Repository Implementations, Identity Services |
| **Application** | `PowerGuard.Application` | CQRS Commands/Queries, MediatR Handlers, Input Validation, Strategy Interface Definitions |
| **Domain** | `PowerGuard.Domain` | Core Business Models, Aggregate Roots, Custom Domain Exceptions, Enums |

### Key Design Patterns & Practices Implemented

1. **CQRS Pattern (via MediatR):** Separates system read queries from write commands. Write operations (`LogConsumptionCommand`) go through specific write paths without locking read queries (`GetDepartmentMetricsQuery`), increasing database performance and scalability.
   
2. **Strategy Design Pattern:** Encapsulates different energy evaluation algorithms. The engine switches evaluation strategies at runtime depending on the department type or active factory shift, ensuring high extensibility without modifying existing core classes (Open-Closed Principle).
   
3. **Repository & Unit of Work Patterns:** Abstracts data persistence mechanisms, ensuring the application layer interacts with an in-memory collection abstraction while handling multi-entity transaction saves inside a single transactional block safely.

---

## 🔒 Authentication & Security Architecture

* **Stateless Token Exchange:** Authentication is managed via cryptographic **JWT** tokens passed through HTTP Authorization headers.
* **Refresh Token Rotation:** Mitigates replay attacks. When a JWT expires, the frontend presents a single-use Refresh Token. The backend validates it, deletes the old instance, rotates the keys, and issues a new pair.
* **Role-Based Authorization:** Endpoints are guarded with granular attributes (e.g., `[Authorize(Roles = "Admin, Manager")]`), shielding critical infrastructure configuration from baseline user roles.

---

## 🔌 API Architecture Overview

All endpoints follow predictable RESTful structures utilizing correct HTTP verb semantics and standardized JSON response envelopes.

| HTTP Method | Endpoint Path | Primary Purpose | Role Authorization |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/Auth/register` | Registers enterprise managers or system roles | Admin Only |
| **POST** | `/api/Auth/login` | Validates credentials; returns JWT & Refresh Token | Public |
| **POST** | `/api/Auth/refresh` | Rotates expired JWT via active refresh token | Public |
| **GET** | `/api/Dashboard/admin` | Fetches global enterprise logs, users, and configurations | Admin Only |
| **GET** | `/api/Dashboard/factory/{id}` | Aggregates localized zone metrics and cost histories | Factory Manager |
| **GET** | `/api/Dashboard/department/{id}`| Retrieves live consumption statistics & active thresholds | All Authenticated |
| **POST** | `/api/Metrics/log` | Ingests new consumption entries (V, I, kWh) | Admin / Employee |

---

## 📂 Project Folder Structure

* **.github/workflows/**: GitHub Actions CI/CD deployment configuration pipelines
* **PowerGuard.Domain/**: Core Business Models, Aggregate Roots, Custom Domain Exceptions
* **PowerGuard.Application/**: CQRS Commands/Queries, MediatR Handlers, Validation & Strategy Definitions
* **PowerGuard.Infrastructure/**: EF Core, Migrations, ApplicationDbContext, Identity Service Implementations
* **PowerGuard.WebAPI/**: Controllers, SignalR Hub Infrastructure, App Middleware, AppSettings

---

## 💻 Local Execution & Setup

### Prerequisites
* .NET 8.0 SDK
* SQL Server (Express or LocalDB instance)

### Setup Steps

1. **Clone Project Core:**
   Run the following command in your terminal to clone the repository:
   `git clone https://github.com/shahdayman315315/PowerGuard.git`

2. **Navigate to Project Directory:**
   `cd PowerGuard`

3. **Configure Database Connection:**
   Navigate to `PowerGuard.WebAPI/appsettings.json` and adjust the `ConnectionStrings:DefaultConnection` to match your local SQL Server instance credentials.

4. **Execute Database Updates:**
   Run the EF Core command to construct your tables and execute schema seeding:
   `dotnet ef database update --project PowerGuard.Infrastructure --startup-project PowerGuard.WebAPI`

5. **Launch Backend Service:**
   `dotnet run --project PowerGuard.WebAPI`
   
*After running, open `https://localhost:7193/swagger` inside your web browser to explore and interact with the endpoints.*

---

## ☁️ Continuous Integration & Deployment (CI/CD)

* **Deployment Target:** Hosted live on **Microsoft Azure App Services**, utilizing an isolated **Azure SQL Database** instances layer.
* **Pipeline Automation:** Fully automated via **GitHub Actions** workflows (located in `.github/workflows`). Every Push or Pull Request targeting the `main` branch initializes a multi-stage runner that builds the codebase and pipelines compilation packages straight to Azure production slots seamlessly.

---

## 📈 Future System Roadmap

* **Distributed State Caching:** Introducing a **Redis** layer to cache static organizational dashboards and reduce database access.
* **Database Optimization:** Implementing database table indexes on high-frequency metric log tables to maintain microsecond performance.
* **IoT Gateway Transition:** Enhancing the current API to directly support automated message streams from MQTT brokers.

---
*Developed by Shahd Ayman – Backend Software Engineer*
