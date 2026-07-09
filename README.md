# EasyPay - Payroll Management System

EasyPay is a comprehensive, full-stack Payroll Management System designed to handle employee onboarding, leave management, timesheets, and automated payroll processing. 

The application is built following **Clean Architecture** principles on the backend and features a modern, responsive single-page application (SPA) on the frontend. It includes a fully dockerized deployment setup and a robust CI/CD pipeline.

---

## 🚀 Tech Stack

### Backend
- **Framework:** .NET 8 (ASP.NET Core Web API)
- **Architecture:** Clean Architecture
- **Database:** Microsoft SQL Server 2022
- **ORM:** Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens) with BCrypt Password Hashing
- **Testing:** NUnit, Moq, Coverlet (Code Coverage)
- **Code Quality:** SonarQube integration

### Frontend
- **Framework:** React 18+ (via Vite)
- **Styling:** CSS Modules / Modern UI
- **Routing:** React Router

### DevOps & Infrastructure
- **Containerization:** Docker & Docker Compose
- **CI/CD:** Jenkins (Declarative Pipeline)
- **Quality Gates:** SonarQube

---

## 📁 Project Structure

```
EasyPay/
├── EasyPay backend/       # .NET 8 Web API (Clean Architecture: API, Core, Infrastructure, Tests)
├── EasyPay frontend/      # React Application
├── Jenkinsfile            # Automated CI/CD Pipeline definition
├── docker-compose.yml     # Multi-container deployment setup
├── .env.example           # Environment variables template
└── README.md              # You are here
```

---

## 🛠️ Getting Started (Docker / Production Mode)

The easiest way to run the entire application (Frontend, Backend, and SQL Server) is using Docker Compose.

### Prerequisites
- [Docker](https://www.docker.com/) & [Docker Compose](https://docs.docker.com/compose/)

### Steps
1. **Clone the repository:**
   ```bash
   git clone https://github.com/suganworks/EasyPay.git
   cd EasyPay
   ```
2. **Setup Environment Variables:**
   Copy the example environment file and create your own `.env` file.
   ```bash
   cp .env.example .env
   ```
   *(Optionally, edit `.env` to change the default passwords if desired).*

3. **Start the application:**
   ```bash
   docker-compose up -d --build
   ```

4. **Access the application:**
   - **Frontend:** http://localhost:3000
   - **Backend API Swagger:** http://localhost:5000/swagger

---

## 💻 Local Development Setup

If you want to run the frontend and backend separately for development purposes:

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or Docker container)

### Backend Setup
1. Update the `DefaultConnection` string in `EasyPay backend/EasyPay.API/appsettings.Development.json` to point to your local SQL Server.
2. Open a terminal in `EasyPay backend/EasyPay.API`.
3. Run the migrations to build the database:
   ```bash
   dotnet ef database update
   ```
   *(Note: The application is also configured to automatically migrate on startup).*
4. Run the API:
   ```bash
   dotnet run
   ```

### Frontend Setup
1. Open a terminal in `EasyPay frontend`.
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the Vite development server:
   ```bash
   npm run dev
   ```
4. Open the provided localhost URL (usually `http://localhost:5173`).

---

## 🔄 CI/CD Pipeline (Jenkins)

The project includes a `Jenkinsfile` that automates the build, test, analysis, and deployment process.

**Pipeline Stages:**
1. **Checkout:** Pulls the latest code from the repository and copies `.env.example` to `.env`.
2. **Backend Build & SonarQube:** Installs the SonarScanner tool, restores NuGet packages, builds the `.sln`, runs unit tests (NUnit) generating code coverage, and publishes results to a local SonarQube server for quality gate checks.
3. **Build Frontend:** Installs NPM dependencies and builds the React production bundle via Vite.
4. **Docker Build:** Rebuilds the frontend and backend Docker container images.
5. **Deploy:** Spins up the SQL Server, API, and Frontend containers using `docker-compose up -d`.

---

## 🧪 Default Test Accounts

When the database is created, it is seeded with sample users. You can log in using any of the following credentials (all use the same password by default).

**Password:** `EasyPay@123!`

| Role | Username | Email |
|------|----------|-------|
| **Admin** | `admin` | `admin@easypay.com` |
| **HR Manager** | `amitabh.bachchan` | `amitabh.bachchan@easypay.in` |
| **Payroll Processor** | `rajinikanth.superstar` | `rajinikanth@easypay.in` |
| **Manager** | `aamir.khan`, `shahrukh.khan` | `aamir.khan@easypay.in`, `shahrukh.khan@easypay.in` |
| **Employee** | `sachin.tendulkar`, `virat.kohli`, `ms.dhoni`, `alia.bhatt` | `sachin.tendulkar@easypay.in`, `virat.kohli@easypay.in`, `ms.dhoni@easypay.in`, `alia.bhatt@easypay.in` |

*(Note: The actual password seed is securely hashed via BCrypt).*
