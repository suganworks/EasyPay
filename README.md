# EasyPay — Payroll Management System

EasyPay is a payroll management system developed using ASP.NET Core 8 and Clean Architecture principles. The application helps manage employees, payroll processing, leave requests, timesheets, benefits, and reporting through a secure REST API.

The project was developed as a case study to demonstrate real-world backend development practices, including layered architecture, authentication, exception handling, logging, unit testing, and database-driven application development.

---

## 🧱 Project Structure

```text
EasyPay/
├── EasyPay.API/             # Controllers, Middleware, Swagger, JWT Auth
├── EasyPay.Core/            # Entities, Interfaces, DTOs, Custom Exceptions
├── EasyPay.Infrastructure/  # EF Core Repositories, Services, Email, Logging
└── EasyPay.Tests/           # NUnit Unit Tests
```

---

## ⚙️ Prerequisites

Make sure you have the following installed before running the project:

| Tool               | Version                       |
| ------------------ | ----------------------------- |
| .NET SDK           | 8.0 or higher                 |
| SQL Server         | Any edition (Express is fine) |
| Visual Studio 2022 | Community or higher           |

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/suganworks/EasyPay.git
cd EasyPay
```

### 2. Configure the Database

Open `EasyPay.API/appsettings.json` and update the connection string according to your SQL Server setup:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=EasyPayDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Replace `YOUR_SERVER_NAME` with your SQL Server instance name (for example, `localhost` or `DESKTOP-ABC\SQLEXPRESS`).

### 3. Apply Database Migrations

Open the project in Visual Studio 2022 and launch the Package Manager Console:

```text
Tools → NuGet Package Manager → Package Manager Console
```

Set the default project to `EasyPay.Infrastructure` and run:

```powershell
Update-Database
```

This will create the `EasyPayDB` database and generate all required tables.

### 4. Run the Application

Run the application using Visual Studio (F5) or:

```bash
cd EasyPay.API
dotnet run
```

The API will be available at:

```text
https://localhost:7001
http://localhost:5001
```

### 5. Access Swagger

Open:

```text
https://localhost:7001/swagger
```

Swagger provides a complete list of available API endpoints and allows testing them directly from the browser.

---

## 🔐 Authentication

EasyPay uses JWT Bearer Authentication for securing API endpoints.

### Using Authentication in Swagger

1. Call `POST /api/v1/auth/login`
2. Copy the generated access token
3. Click the **Authorize** button in Swagger
4. Paste the token
5. Access protected endpoints

### Default Admin Account

```json
{
  "username": "admin",
  "password": "Admin@456!"
}
```

The default administrator account is seeded automatically during database creation.

---

## ✅ Running Tests

Using Visual Studio:

```text
Test → Test Explorer → Run All Tests
```

Using the command line:

```bash
cd EasyPay.Tests
dotnet test
```

The test suite covers authentication, employee management, payroll processing, leave management, timesheets, benefits, reporting, and email services.

---

## 🗂️ Key Features

* Employee Management with designation support
* Payroll Processing with salary calculations and deductions
* Leave Application and Approval Workflow
* Timesheet Submission and Approval
* Employee Benefits Management
* Payroll Register Reporting
* JWT Authentication with Access and Refresh Tokens
* Forgot Password using Email Verification
* Global Exception Handling Middleware
* Structured Logging using Serilog

---

## 🛠️ Tech Stack

| Layer             | Technology                   |
| ----------------- | ---------------------------- |
| Framework         | ASP.NET Core 8               |
| ORM               | Entity Framework Core 8      |
| Database          | SQL Server                   |
| Authentication    | JWT Bearer Tokens            |
| Password Hashing  | BCrypt.Net-Next              |
| Logging           | Serilog                      |
| Object Mapping    | AutoMapper                   |
| API Documentation | Swagger / Swashbuckle        |
| Testing           | NUnit, Moq, FluentAssertions |

---

## 📁 Logs

Application logs are stored inside the `Logs` folder:

```text
EasyPay.API/
└── Logs/
    └── easypay-yyyyMMdd.log
```

Log files are generated automatically and rolled daily.

---

## ⚠️ Note

The `appsettings.json` file is included only for local development and testing purposes.

For production environments, sensitive information such as database connection strings, JWT secret keys, SMTP credentials, and API keys should be stored securely using environment variables, Azure Key Vault, or another secure secrets management solution rather than being committed to source control.

---

## 📬 Email Configuration (Optional)

The Forgot Password functionality supports SMTP email integration.

Update the following section in `appsettings.json` if email functionality is required:

```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": "587",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "FromEmail": "noreply@easypay.com",
  "FromName": "EasyPay Payroll",
  "EnableSsl": "true"
}
```

If SMTP settings are not configured, the application will continue to run normally, but email-based features will be unavailable.

---

## 🏗️ Architecture

EasyPay follows Clean Architecture principles to maintain clear separation of concerns between application layers.

```text
API Layer          → Handles HTTP requests, routing, authentication and middleware
Core Layer         → Contains entities, DTOs, interfaces and business contracts
Infrastructure     → Implements repositories, services, email and logging
Tests              → Contains unit tests for business logic and services
```

Dependencies flow inward toward the Core layer, helping keep business logic independent, maintainable, and testable.
