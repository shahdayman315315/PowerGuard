# PowerGuard EMS (Energy Management System) ⚡

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-blue.svg)]()
[![CQRS](https://img.shields.io/badge/Pattern-CQRS_%2F_MediatR-green.svg)]()
[![Azure Deployed](https://img.shields.io/badge/Deployment-Azure_App_Service-orange.svg)]()

PowerGuard is an Enterprise-grade Industrial Energy Management System (EMS) designed to monitor, analyze, and optimize factory power consumption in real-time. Built on **.NET 8 Web API** following **Clean Architecture** and **Domain-Driven Design (DDD)** principles, the platform processes high-frequency IoT telemetry to prevent costly over-consumption penalties and detect equipment anomalies.

---

## 🚀 Project Overview

In heavy manufacturing industries, energy consumption overhead and peak-load penalties account for massive financial losses. **PowerGuard** bridges the gap between hardware telemetry and industrial operations. By ingesting real-time data from IoT smart meters, the system provides sub-metering insights across hierarchical factory structures, triggers real-time alerts via WebSockets, and evaluates energy efficiency using dynamic statistical strategies.

---

## ✨ Core Features

* **Hierarchical Telemetry Ingestion:** Multi-level monitoring spanning from the entire corporate infrastructure down to individual factory zones, departments, and specific heavy machinery.
* **Real-Time Threshold Alerting:** Instantaneous push notifications driven by **SignalR** when consumption breaches predefined safety thresholds.
* **Dynamic Efficiency Evaluator:** Extensible rule-engine utilizing the **Strategy Design Pattern** to calculate consumption efficiency against production outputs.
* **Enterprise Identity & RBAC:** Secure user onboarding and fine-grained Role-Based Access Control (Admin, Factory Manager, Maintenance Engineer).
* **Resilient Data Pipeline:** Asynchronous, non-blocking telemetry endpoints optimized for continuous IoT data logging.

---

## 🛠️ Tech Stack

* **Backend Framework:** .NET 8 / ASP.NET Core Web API
* **Database & ORM:** Microsoft SQL Server / Azure SQL Core | Entity Framework Core
* **Real-Time Engine:** ASP.NET Core SignalR (WebSockets)
* **Mediation & Messaging:** MediatR (In-Process Memory Bus)
* **Security:** ASP.NET Core Identity | JWT (JSON Web Tokens) with Refresh Token Rotation
* **Testing:** xUnit | FluentAssertions
* **Cloud & DevOps:** Microsoft Azure (App Services, SQL Core) | GitHub Actions (CI/CD)

---

## 🏗️ Architecture & Design Patterns

The backend is engineered using **Clean Architecture** to enforce a strict Separation of Concerns (SoC) and ensure the system remains independent of database frameworks or external UI clients.
