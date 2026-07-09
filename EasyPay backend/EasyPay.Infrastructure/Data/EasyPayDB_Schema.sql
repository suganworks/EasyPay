-- =============================================================
-- EasyPay Payroll Management System - Complete SQL Server Schema
-- Version: 1.0  |  Database: EasyPayDB
-- =============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EasyPayDB')
BEGIN
    CREATE DATABASE EasyPayDB;
END
GO

USE EasyPayDB;
GO

-- =============================================================
-- TABLE: Roles
-- =============================================================
CREATE TABLE Roles (
    RoleId        INT            IDENTITY(1,1) PRIMARY KEY,
    RoleName      NVARCHAR(50)   NOT NULL UNIQUE,
    Description   NVARCHAR(200)  NULL,
    IsActive      BIT            NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);

-- =============================================================
-- TABLE: Users
-- =============================================================
CREATE TABLE Users (
    UserId              INT            IDENTITY(1,1) PRIMARY KEY,
    Username            NVARCHAR(100)  NOT NULL UNIQUE,
    Email               NVARCHAR(255)  NOT NULL UNIQUE,
    PasswordHash        NVARCHAR(500)  NOT NULL,
    RoleId              INT            NOT NULL,
    IsActive            BIT            NOT NULL DEFAULT 1,
    IsEmailVerified     BIT            NOT NULL DEFAULT 0,
    RefreshToken        NVARCHAR(500)  NULL,
    RefreshTokenExpiry  DATETIME2      NULL,
    PasswordResetToken  NVARCHAR(500)  NULL,
    PasswordResetExpiry DATETIME2      NULL,
    LastLoginAt         DATETIME2      NULL,
    FailedLoginAttempts INT            NOT NULL DEFAULT 0,
    LockoutEnd          DATETIME2      NULL,
    CreatedAt           DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

CREATE INDEX IX_Users_Email   ON Users(Email);
CREATE INDEX IX_Users_RoleId  ON Users(RoleId);

-- =============================================================
-- TABLE: Departments
-- =============================================================
CREATE TABLE Departments (
    DepartmentId    INT            IDENTITY(1,1) PRIMARY KEY,
    DepartmentCode  NVARCHAR(20)   NOT NULL UNIQUE,
    DepartmentName  NVARCHAR(100)  NOT NULL,
    Description     NVARCHAR(500)  NULL,
    ManagerUserId   INT            NULL,
    IsActive        BIT            NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy       INT            NULL,
    UpdatedBy       INT            NULL
);

-- =============================================================
-- TABLE: Designations
-- =============================================================
CREATE TABLE Designations (
    DesignationId    INT            IDENTITY(1,1) PRIMARY KEY,
    DesignationCode  NVARCHAR(20)   NOT NULL UNIQUE,
    DesignationName  NVARCHAR(100)  NOT NULL,
    DepartmentId     INT            NOT NULL,
    GradeLevel       NVARCHAR(20)   NULL,
    IsActive         BIT            NOT NULL DEFAULT 1,
    CreatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy        INT            NULL,
    UpdatedBy        INT            NULL,
    CONSTRAINT FK_Designations_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId)
);

-- =============================================================
-- TABLE: Employees
-- =============================================================
CREATE TABLE Employees (
    EmployeeId       INT            IDENTITY(1,1) PRIMARY KEY,
    EmployeeCode     NVARCHAR(20)   NOT NULL UNIQUE,
    UserId           INT            NOT NULL,
    FirstName        NVARCHAR(100)  NOT NULL,
    LastName         NVARCHAR(100)  NOT NULL,
    DateOfBirth      DATE           NOT NULL,
    Gender           NVARCHAR(20)   NOT NULL,
    NationalId       NVARCHAR(50)   NULL,
    PassportNumber   NVARCHAR(50)   NULL,
    Phone            NVARCHAR(20)   NULL,
    AlternatePhone   NVARCHAR(20)   NULL,
    PersonalEmail    NVARCHAR(255)  NULL,
    Address          NVARCHAR(500)  NULL,
    City             NVARCHAR(100)  NULL,
    State            NVARCHAR(100)  NULL,
    PostalCode       NVARCHAR(20)   NULL,
    Country          NVARCHAR(100)  NULL,
    DepartmentId     INT            NOT NULL,
    DesignationId    INT            NOT NULL,
    ManagerId        INT            NULL,
    JoiningDate      DATE           NOT NULL,
    ConfirmationDate DATE           NULL,
    TerminationDate  DATE           NULL,
    EmploymentType   NVARCHAR(50)   NOT NULL DEFAULT 'FullTime',  -- FullTime, PartTime, Contract, Intern
    EmploymentStatus NVARCHAR(50)   NOT NULL DEFAULT 'Active',    -- Active, OnLeave, Terminated, Resigned
    BankName         NVARCHAR(100)  NULL,
    BankAccountNo    NVARCHAR(50)   NULL,
    BankIFSC         NVARCHAR(20)   NULL,
    PanNumber        NVARCHAR(20)   NULL,
    PfNumber         NVARCHAR(50)   NULL,
    EsiNumber        NVARCHAR(50)   NULL,
    TaxWithholding   DECIMAL(5,2)   NOT NULL DEFAULT 0,
    PhotoUrl         NVARCHAR(500)  NULL,
    IsActive         BIT            NOT NULL DEFAULT 1,
    CreatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy        INT            NULL,
    UpdatedBy        INT            NULL,
    CONSTRAINT FK_Employees_Users         FOREIGN KEY (UserId)        REFERENCES Users(UserId),
    CONSTRAINT FK_Employees_Departments   FOREIGN KEY (DepartmentId)  REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_Employees_Designations  FOREIGN KEY (DesignationId) REFERENCES Designations(DesignationId),
    CONSTRAINT FK_Employees_Manager       FOREIGN KEY (ManagerId)     REFERENCES Employees(EmployeeId)
);

CREATE INDEX IX_Employees_UserId       ON Employees(UserId);
CREATE INDEX IX_Employees_DepartmentId ON Employees(DepartmentId);
CREATE INDEX IX_Employees_ManagerId    ON Employees(ManagerId);
CREATE INDEX IX_Employees_Status       ON Employees(EmploymentStatus);

-- =============================================================
-- TABLE: PayrollPolicies
-- =============================================================
CREATE TABLE PayrollPolicies (
    PolicyId           INT             IDENTITY(1,1) PRIMARY KEY,
    PolicyName         NVARCHAR(100)   NOT NULL,
    EffectiveFrom      DATE            NOT NULL,
    EffectiveTo        DATE            NULL,
    PayFrequency       NVARCHAR(20)    NOT NULL DEFAULT 'Monthly',  -- Weekly, BiWeekly, Monthly
    OvertimeRate       DECIMAL(5,2)    NOT NULL DEFAULT 1.5,
    PfEmployeeRate     DECIMAL(5,2)    NOT NULL DEFAULT 12.0,
    PfEmployerRate     DECIMAL(5,2)    NOT NULL DEFAULT 12.0,
    EsiEmployeeRate    DECIMAL(5,2)    NOT NULL DEFAULT 0.75,
    EsiEmployerRate    DECIMAL(5,2)    NOT NULL DEFAULT 3.25,
    ProfessionalTax    DECIMAL(10,2)   NOT NULL DEFAULT 200.0,
    GratuityRate       DECIMAL(5,2)    NOT NULL DEFAULT 4.81,
    WorkingDaysMonth   INT             NOT NULL DEFAULT 26,
    WorkingHoursDay    DECIMAL(4,2)    NOT NULL DEFAULT 8.0,
    IsActive           BIT             NOT NULL DEFAULT 1,
    Description        NVARCHAR(500)   NULL,
    CreatedAt          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy          INT             NULL,
    UpdatedBy          INT             NULL
);

-- =============================================================
-- TABLE: SalaryStructures
-- =============================================================
CREATE TABLE SalaryStructures (
    SalaryStructureId  INT             IDENTITY(1,1) PRIMARY KEY,
    EmployeeId         INT             NOT NULL,
    PolicyId           INT             NOT NULL,
    EffectiveFrom      DATE            NOT NULL,
    EffectiveTo        DATE            NULL,
    BasicSalary        DECIMAL(18,2)   NOT NULL,
    HRA                DECIMAL(18,2)   NOT NULL DEFAULT 0,
    ConveyanceAllowance DECIMAL(18,2)  NOT NULL DEFAULT 0,
    MedicalAllowance   DECIMAL(18,2)   NOT NULL DEFAULT 0,
    SpecialAllowance   DECIMAL(18,2)   NOT NULL DEFAULT 0,
    LTA                DECIMAL(18,2)   NOT NULL DEFAULT 0,
    OtherAllowances    DECIMAL(18,2)   NOT NULL DEFAULT 0,
    GrossSalary        AS (BasicSalary + HRA + ConveyanceAllowance + MedicalAllowance
                           + SpecialAllowance + LTA + OtherAllowances) PERSISTED,
    IsActive           BIT             NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy          INT             NULL,
    UpdatedBy          INT             NULL,
    CONSTRAINT FK_SalaryStructures_Employees      FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_SalaryStructures_PayrollPolicies FOREIGN KEY (PolicyId)   REFERENCES PayrollPolicies(PolicyId)
);

CREATE INDEX IX_SalaryStructures_EmployeeId ON SalaryStructures(EmployeeId);

-- =============================================================
-- TABLE: LeaveTypes
-- =============================================================
CREATE TABLE LeaveTypes (
    LeaveTypeId      INT            IDENTITY(1,1) PRIMARY KEY,
    LeaveTypeName    NVARCHAR(50)   NOT NULL UNIQUE,
    LeaveCode        NVARCHAR(10)   NOT NULL UNIQUE,
    MaxDaysPerYear   INT            NOT NULL DEFAULT 0,
    IsCarryForward   BIT            NOT NULL DEFAULT 0,
    MaxCarryForward  INT            NOT NULL DEFAULT 0,
    IsPaid           BIT            NOT NULL DEFAULT 1,
    IsActive         BIT            NOT NULL DEFAULT 1,
    Description      NVARCHAR(500)  NULL,
    CreatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);

-- =============================================================
-- TABLE: LeaveRequests
-- =============================================================
CREATE TABLE LeaveRequests (
    LeaveRequestId   INT            IDENTITY(1,1) PRIMARY KEY,
    EmployeeId       INT            NOT NULL,
    LeaveTypeId      INT            NOT NULL,
    FromDate         DATE           NOT NULL,
    ToDate           DATE           NOT NULL,
    TotalDays        INT            NOT NULL,
    Reason           NVARCHAR(1000) NULL,
    Status           NVARCHAR(20)   NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected, Cancelled
    ApprovedById     INT            NULL,
    ApprovedAt       DATETIME2      NULL,
    RejectionReason  NVARCHAR(500)  NULL,
    IsHalfDay        BIT            NOT NULL DEFAULT 0,
    HalfDayType      NVARCHAR(10)   NULL, -- AM, PM
    CreatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_LeaveRequests_Employees     FOREIGN KEY (EmployeeId)   REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_LeaveRequests_LeaveTypes    FOREIGN KEY (LeaveTypeId)  REFERENCES LeaveTypes(LeaveTypeId),
    CONSTRAINT FK_LeaveRequests_ApprovedBy    FOREIGN KEY (ApprovedById) REFERENCES Employees(EmployeeId)
);

CREATE INDEX IX_LeaveRequests_EmployeeId ON LeaveRequests(EmployeeId);
CREATE INDEX IX_LeaveRequests_Status     ON LeaveRequests(Status);
CREATE INDEX IX_LeaveRequests_Dates      ON LeaveRequests(FromDate, ToDate);

-- =============================================================
-- TABLE: Timesheets
-- =============================================================
CREATE TABLE Timesheets (
    TimesheetId    INT            IDENTITY(1,1) PRIMARY KEY,
    EmployeeId     INT            NOT NULL,
    WorkDate       DATE           NOT NULL,
    CheckIn        TIME           NULL,
    CheckOut       TIME           NULL,
    HoursWorked    DECIMAL(5,2)   NOT NULL DEFAULT 0,
    OvertimeHours  DECIMAL(5,2)   NOT NULL DEFAULT 0,
    Status         NVARCHAR(20)   NOT NULL DEFAULT 'Pending',  -- Pending, Approved, Rejected
    ApprovedById   INT            NULL,
    ApprovedAt     DATETIME2      NULL,
    Notes          NVARCHAR(500)  NULL,
    CreatedAt      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Timesheets_Employees   FOREIGN KEY (EmployeeId)  REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_Timesheets_ApprovedBy  FOREIGN KEY (ApprovedById) REFERENCES Employees(EmployeeId),
    CONSTRAINT UQ_Timesheets_EmpDate     UNIQUE (EmployeeId, WorkDate)
);

CREATE INDEX IX_Timesheets_EmployeeId ON Timesheets(EmployeeId);
CREATE INDEX IX_Timesheets_WorkDate   ON Timesheets(WorkDate);

-- =============================================================
-- TABLE: Benefits
-- =============================================================
CREATE TABLE Benefits (
    BenefitId      INT            IDENTITY(1,1) PRIMARY KEY,
    BenefitName    NVARCHAR(100)  NOT NULL,
    BenefitCode    NVARCHAR(20)   NOT NULL UNIQUE,
    BenefitType    NVARCHAR(50)   NOT NULL,  -- Health, Insurance, Retirement, Transport, Meal
    Amount         DECIMAL(18,2)  NOT NULL DEFAULT 0,
    IsPercentage   BIT            NOT NULL DEFAULT 0,
    Description    NVARCHAR(500)  NULL,
    IsActive       BIT            NOT NULL DEFAULT 1,
    CreatedAt      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);

-- =============================================================
-- TABLE: EmployeeBenefits
-- =============================================================
CREATE TABLE EmployeeBenefits (
    EmployeeBenefitId  INT            IDENTITY(1,1) PRIMARY KEY,
    EmployeeId         INT            NOT NULL,
    BenefitId          INT            NOT NULL,
    EffectiveFrom      DATE           NOT NULL,
    EffectiveTo        DATE           NULL,
    OverrideAmount     DECIMAL(18,2)  NULL,
    IsActive           BIT            NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy          INT            NULL,
    CONSTRAINT FK_EmployeeBenefits_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_EmployeeBenefits_Benefits  FOREIGN KEY (BenefitId)  REFERENCES Benefits(BenefitId)
);

CREATE INDEX IX_EmployeeBenefits_EmployeeId ON EmployeeBenefits(EmployeeId);

-- =============================================================
-- TABLE: Payrolls
-- =============================================================
CREATE TABLE Payrolls (
    PayrollId         INT             IDENTITY(1,1) PRIMARY KEY,
    EmployeeId        INT             NOT NULL,
    PolicyId          INT             NOT NULL,
    SalaryStructureId INT             NOT NULL,
    PayPeriodStart    DATE            NOT NULL,
    PayPeriodEnd      DATE            NOT NULL,
    PaymentDate       DATE            NULL,
    WorkingDays       INT             NOT NULL DEFAULT 0,
    PresentDays       INT             NOT NULL DEFAULT 0,
    LeaveDays         INT             NOT NULL DEFAULT 0,
    OvertimeHours     DECIMAL(8,2)    NOT NULL DEFAULT 0,
    BasicSalary       DECIMAL(18,2)   NOT NULL DEFAULT 0,
    HRA               DECIMAL(18,2)   NOT NULL DEFAULT 0,
    ConveyanceAllow   DECIMAL(18,2)   NOT NULL DEFAULT 0,
    MedicalAllow      DECIMAL(18,2)   NOT NULL DEFAULT 0,
    SpecialAllow      DECIMAL(18,2)   NOT NULL DEFAULT 0,
    OtherAllowances   DECIMAL(18,2)   NOT NULL DEFAULT 0,
    OvertimePay       DECIMAL(18,2)   NOT NULL DEFAULT 0,
    BonusAmount       DECIMAL(18,2)   NOT NULL DEFAULT 0,
    GrossEarnings     DECIMAL(18,2)   NOT NULL DEFAULT 0,
    PfEmployee        DECIMAL(18,2)   NOT NULL DEFAULT 0,
    PfEmployer        DECIMAL(18,2)   NOT NULL DEFAULT 0,
    EsiEmployee       DECIMAL(18,2)   NOT NULL DEFAULT 0,
    EsiEmployer       DECIMAL(18,2)   NOT NULL DEFAULT 0,
    ProfessionalTax   DECIMAL(18,2)   NOT NULL DEFAULT 0,
    IncomeTax         DECIMAL(18,2)   NOT NULL DEFAULT 0,
    OtherDeductions   DECIMAL(18,2)   NOT NULL DEFAULT 0,
    TotalDeductions   DECIMAL(18,2)   NOT NULL DEFAULT 0,
    NetSalary         DECIMAL(18,2)   NOT NULL DEFAULT 0,
    Status            NVARCHAR(20)    NOT NULL DEFAULT 'Draft',  -- Draft, Pending, Approved, Paid, Cancelled
    ProcessedById     INT             NULL,
    ProcessedAt       DATETIME2       NULL,
    ApprovedById      INT             NULL,
    ApprovedAt        DATETIME2       NULL,
    Remarks           NVARCHAR(1000)  NULL,
    CreatedAt         DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt         DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy         INT             NULL,
    UpdatedBy         INT             NULL,
    CONSTRAINT FK_Payrolls_Employees        FOREIGN KEY (EmployeeId)        REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_Payrolls_Policies         FOREIGN KEY (PolicyId)          REFERENCES PayrollPolicies(PolicyId),
    CONSTRAINT FK_Payrolls_SalaryStructures FOREIGN KEY (SalaryStructureId) REFERENCES SalaryStructures(SalaryStructureId)
);

CREATE INDEX IX_Payrolls_EmployeeId   ON Payrolls(EmployeeId);
CREATE INDEX IX_Payrolls_Status       ON Payrolls(Status);
CREATE INDEX IX_Payrolls_PayPeriod    ON Payrolls(PayPeriodStart, PayPeriodEnd);

-- =============================================================
-- TABLE: Notifications
-- =============================================================
CREATE TABLE Notifications (
    NotificationId   INT             IDENTITY(1,1) PRIMARY KEY,
    UserId           INT             NOT NULL,
    Title            NVARCHAR(200)   NOT NULL,
    Message          NVARCHAR(1000)  NOT NULL,
    NotificationType NVARCHAR(50)    NOT NULL DEFAULT 'Info',  -- Info, Warning, Success, Error
    ReferenceType    NVARCHAR(50)    NULL,  -- Payroll, Leave, Timesheet, etc.
    ReferenceId      INT             NULL,
    IsRead           BIT             NOT NULL DEFAULT 0,
    ReadAt           DATETIME2       NULL,
    CreatedAt        DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE INDEX IX_Notifications_UserId  ON Notifications(UserId);
CREATE INDEX IX_Notifications_IsRead  ON Notifications(IsRead);

-- =============================================================
-- TABLE: AuditLogs
-- =============================================================
CREATE TABLE AuditLogs (
    AuditLogId     BIGINT          IDENTITY(1,1) PRIMARY KEY,
    UserId         INT             NULL,
    UserEmail      NVARCHAR(255)   NULL,
    Action         NVARCHAR(100)   NOT NULL,  -- CREATE, UPDATE, DELETE, LOGIN, LOGOUT
    EntityName     NVARCHAR(100)   NULL,
    EntityId       NVARCHAR(50)    NULL,
    OldValues      NVARCHAR(MAX)   NULL,
    NewValues      NVARCHAR(MAX)   NULL,
    IPAddress      NVARCHAR(50)    NULL,
    UserAgent      NVARCHAR(500)   NULL,
    IsSuccess      BIT             NOT NULL DEFAULT 1,
    ErrorMessage   NVARCHAR(1000)  NULL,
    CreatedAt      DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_AuditLogs_UserId     ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Action     ON AuditLogs(Action);
CREATE INDEX IX_AuditLogs_EntityName ON AuditLogs(EntityName);
CREATE INDEX IX_AuditLogs_CreatedAt  ON AuditLogs(CreatedAt);

-- =============================================================
-- SEED DATA
-- =============================================================

-- Seed Roles
INSERT INTO Roles (RoleName, Description) VALUES
('Admin',            'System Administrator with full access'),
('HRManager',        'HR Manager with employee and payroll management access'),
('PayrollProcessor', 'Payroll Processor with payroll calculation access'),
('Employee',         'Regular employee with self-service access'),
('Manager',          'Team Manager with team oversight access');

-- Seed Leave Types
INSERT INTO LeaveTypes (LeaveTypeName, LeaveCode, MaxDaysPerYear, IsCarryForward, MaxCarryForward, IsPaid, Description) VALUES
('Casual Leave',      'CL',  12, 0, 0,  1, 'General purpose casual leave'),
('Sick Leave',        'SL',  12, 0, 0,  1, 'Medical/health related leave'),
('Privilege Leave',   'PL',  15, 1, 15, 1, 'Annual earned privilege leave'),
('Maternity Leave',   'ML',  90, 0, 0,  1, 'Maternity leave for female employees'),
('Paternity Leave',   'PAT', 15, 0, 0,  1, 'Paternity leave for male employees'),
('Loss of Pay',       'LOP',  0, 0, 0,  0, 'Leave without pay'),
('Compensatory Off',  'CO',   0, 1, 5,  1, 'Compensatory off for extra work');

-- Seed Default Payroll Policy
INSERT INTO PayrollPolicies (PolicyName, EffectiveFrom, PayFrequency, OvertimeRate, PfEmployeeRate, PfEmployerRate,
    EsiEmployeeRate, EsiEmployerRate, ProfessionalTax, GratuityRate, WorkingDaysMonth, WorkingHoursDay, Description)
VALUES ('Standard Policy 2024', '2024-01-01', 'Monthly', 1.5, 12.0, 12.0, 0.75, 3.25, 200.0, 4.81, 26, 8.0,
        'Default payroll policy for all employees');

-- Seed Admin User (Password: Admin@123!)
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, IsEmailVerified)
VALUES ('admin', 'admin@easypay.com',
        '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', -- Admin@123!
        (SELECT RoleId FROM Roles WHERE RoleName = 'Admin'), 1, 1);

-- Seed Default Department
INSERT INTO Departments (DepartmentCode, DepartmentName, Description, CreatedBy)
VALUES ('IT', 'Information Technology', 'IT Department', 1),
       ('HR', 'Human Resources',        'HR Department', 1),
       ('FIN','Finance',                 'Finance Department', 1),
       ('OPS','Operations',              'Operations Department', 1);

-- Seed Default Designations
INSERT INTO Designations (DesignationCode, DesignationName, DepartmentId, GradeLevel)
VALUES
('SWE',  'Software Engineer',         1, 'L3'),
('SSE',  'Senior Software Engineer',  1, 'L4'),
('TL',   'Tech Lead',                 1, 'L5'),
('ARCH', 'Architect',                 1, 'L6'),
('HRE',  'HR Executive',              2, 'L2'),
('HRM',  'HR Manager',                2, 'L5'),
('FA',   'Finance Analyst',           3, 'L3'),
('FM',   'Finance Manager',           3, 'L5');

GO

PRINT 'EasyPayDB schema created successfully.';
