-- =============================================================
-- EasyPay Payroll Management System - COMPLETE SEED DATA SCRIPT
-- Period: July 2024 - July 2026 (2 Years)
-- Employees: ~50 Indian Celebrity Names
-- Special Emails:
--   HR Manager  : grootthe38@gmail.com
--   Employee    : suganprabhakaran196@gmail.com
-- Default Password for ALL users: EasyPay@123!
-- BCrypt hash of EasyPay@123! (cost 11):
--   $2a$11$K7L/fE5nZrD3p0NxNzH8JeQfW1vX2sYmR4uB6tC9dA0kP7gHiMnLq
-- =============================================================

USE EasyPayDB;
GO

-- =============================================================
-- STEP 0: CLEAN ALL TABLES (in FK-safe order)
-- =============================================================
DELETE FROM AuditLogs;
DELETE FROM Notifications;
DELETE FROM Timesheets;
DELETE FROM LeaveRequests;
DELETE FROM Payrolls;
DELETE FROM EmployeeBenefits;
DELETE FROM SalaryStructures;
DELETE FROM Benefits;
DELETE FROM Employees;
DELETE FROM SalaryStructures; -- double ensure
DELETE FROM PayrollPolicies;
DELETE FROM Designations;
DELETE FROM Departments;
DELETE FROM Users;
DELETE FROM LeaveTypes;
DELETE FROM Roles;

-- Reset identity seeds
DBCC CHECKIDENT ('Roles', RESEED, 0);
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Departments', RESEED, 0);
DBCC CHECKIDENT ('Designations', RESEED, 0);
DBCC CHECKIDENT ('Employees', RESEED, 0);
DBCC CHECKIDENT ('PayrollPolicies', RESEED, 0);
DBCC CHECKIDENT ('SalaryStructures', RESEED, 0);
DBCC CHECKIDENT ('Benefits', RESEED, 0);
DBCC CHECKIDENT ('EmployeeBenefits', RESEED, 0);
DBCC CHECKIDENT ('LeaveTypes', RESEED, 0);
DBCC CHECKIDENT ('LeaveRequests', RESEED, 0);
DBCC CHECKIDENT ('Timesheets', RESEED, 0);
DBCC CHECKIDENT ('Payrolls', RESEED, 0);
DBCC CHECKIDENT ('Notifications', RESEED, 0);

GO

-- =============================================================
-- STEP 1: ROLES
-- =============================================================
INSERT INTO Roles (RoleName, Description, IsActive, CreatedAt, UpdatedAt) VALUES
('Admin',            'System Administrator with full access',              1, '2024-01-01', '2024-01-01'),
('HRManager',        'HR Manager with employee and payroll management',    1, '2024-01-01', '2024-01-01'),
('PayrollProcessor', 'Payroll Processor with payroll calculation access',  1, '2024-01-01', '2024-01-01'),
('Employee',         'Regular employee with self-service access',          1, '2024-01-01', '2024-01-01'),
('Manager',          'Team Manager with team oversight access',            1, '2024-01-01', '2024-01-01');
GO

-- =============================================================
-- STEP 2: LEAVE TYPES
-- =============================================================
INSERT INTO LeaveTypes (LeaveTypeName, LeaveCode, MaxDaysPerYear, IsCarryForward, MaxCarryForward, IsPaid, IsActive, Description, CreatedAt, UpdatedAt) VALUES
('Casual Leave',     'CL',  12, 0, 0,  1, 1, 'General purpose casual leave',        '2024-01-01', '2024-01-01'),
('Sick Leave',       'SL',  12, 0, 0,  1, 1, 'Medical/health related leave',         '2024-01-01', '2024-01-01'),
('Privilege Leave',  'PL',  15, 1, 15, 1, 1, 'Annual earned privilege leave',        '2024-01-01', '2024-01-01'),
('Maternity Leave',  'ML',  90, 0, 0,  1, 1, 'Maternity leave for female employees', '2024-01-01', '2024-01-01'),
('Paternity Leave',  'PAT', 15, 0, 0,  1, 1, 'Paternity leave for male employees',   '2024-01-01', '2024-01-01'),
('Loss of Pay',      'LOP',  0, 0, 0,  0, 1, 'Leave without pay',                   '2024-01-01', '2024-01-01'),
('Compensatory Off', 'CO',   0, 1, 5,  1, 1, 'Compensatory off for extra work',      '2024-01-01', '2024-01-01');
GO

-- =============================================================
-- STEP 3: PAYROLL POLICIES
-- =============================================================
INSERT INTO PayrollPolicies (PolicyName, EffectiveFrom, EffectiveTo, PayFrequency, OvertimeRate,
    PfEmployeeRate, PfEmployerRate, EsiEmployeeRate, EsiEmployerRate, ProfessionalTax,
    GratuityRate, WorkingDaysMonth, WorkingHoursDay, IsActive, Description, CreatedAt, UpdatedAt, CreatedBy)
VALUES
('Standard Policy FY2024-25', '2024-07-01', '2025-06-30', 'Monthly', 1.5, 12.0, 12.0, 0.75, 3.25, 200.0, 4.81, 26, 8.0, 1,
 'Standard payroll policy for FY 2024-25', '2024-07-01', '2024-07-01', 1),
('Standard Policy FY2025-26', '2025-07-01', NULL,         'Monthly', 1.5, 12.0, 12.0, 0.75, 3.25, 200.0, 4.81, 26, 8.0, 1,
 'Standard payroll policy for FY 2025-26', '2025-07-01', '2025-07-01', 1);
GO

-- =============================================================
-- STEP 4: DEPARTMENTS (8 departments)
-- =============================================================
INSERT INTO Departments (DepartmentCode, DepartmentName, Description, IsActive, CreatedAt, UpdatedAt, CreatedBy) VALUES
('IT',   'Information Technology', 'Software development and IT infrastructure',        1, '2024-07-01', '2024-07-01', 1),
('HR',   'Human Resources',        'Recruitment, onboarding, employee relations',        1, '2024-07-01', '2024-07-01', 1),
('FIN',  'Finance & Accounts',     'Financial planning, accounting and compliance',      1, '2024-07-01', '2024-07-01', 1),
('OPS',  'Operations',             'Business operations and process management',         1, '2024-07-01', '2024-07-01', 1),
('MKT',  'Marketing',              'Brand management, campaigns and digital marketing',  1, '2024-07-01', '2024-07-01', 1),
('SAL',  'Sales',                  'Revenue generation and client relationship mgmt',    1, '2024-07-01', '2024-07-01', 1),
('LEGAL','Legal & Compliance',     'Legal advisory and regulatory compliance',           1, '2024-07-01', '2024-07-01', 1),
('PAY',  'Payroll',                'Payroll processing and statutory compliance',        1, '2024-07-01', '2024-07-01', 1);
GO

-- =============================================================
-- STEP 5: DESIGNATIONS (per department, 4-5 each = 35 designations)
-- =============================================================
-- IT (DeptId=1)
INSERT INTO Designations (DesignationCode, DesignationName, DepartmentId, GradeLevel, IsActive, CreatedAt, UpdatedAt) VALUES
('SWE',   'Software Engineer',          1, 'L3', 1, '2024-07-01', '2024-07-01'),
('SSE',   'Senior Software Engineer',   1, 'L4', 1, '2024-07-01', '2024-07-01'),
('TL',    'Tech Lead',                  1, 'L5', 1, '2024-07-01', '2024-07-01'),
('ARCH',  'Solution Architect',         1, 'L6', 1, '2024-07-01', '2024-07-01'),
('VP_IT', 'VP Engineering',             1, 'L7', 1, '2024-07-01', '2024-07-01'),
-- HR (DeptId=2)
('HRE',   'HR Executive',               2, 'L2', 1, '2024-07-01', '2024-07-01'),
('HRS',   'HR Specialist',              2, 'L3', 1, '2024-07-01', '2024-07-01'),
('HRM',   'HR Manager',                 2, 'L5', 1, '2024-07-01', '2024-07-01'),
('HRBP',  'HR Business Partner',        2, 'L4', 1, '2024-07-01', '2024-07-01'),
('CHRO',  'Chief HR Officer',           2, 'L7', 1, '2024-07-01', '2024-07-01'),
-- Finance (DeptId=3)
('FA',    'Finance Analyst',            3, 'L3', 1, '2024-07-01', '2024-07-01'),
('SFA',   'Senior Finance Analyst',     3, 'L4', 1, '2024-07-01', '2024-07-01'),
('FM',    'Finance Manager',            3, 'L5', 1, '2024-07-01', '2024-07-01'),
('CFO',   'Chief Financial Officer',    3, 'L7', 1, '2024-07-01', '2024-07-01'),
-- Operations (DeptId=4)
('OA',    'Operations Analyst',         4, 'L3', 1, '2024-07-01', '2024-07-01'),
('OM',    'Operations Manager',         4, 'L5', 1, '2024-07-01', '2024-07-01'),
('OD',    'Operations Director',        4, 'L6', 1, '2024-07-01', '2024-07-01'),
-- Marketing (DeptId=5)
('ME',    'Marketing Executive',        5, 'L2', 1, '2024-07-01', '2024-07-01'),
('MS',    'Marketing Specialist',       5, 'L3', 1, '2024-07-01', '2024-07-01'),
('MM',    'Marketing Manager',          5, 'L5', 1, '2024-07-01', '2024-07-01'),
('CMO',   'Chief Marketing Officer',    5, 'L7', 1, '2024-07-01', '2024-07-01'),
-- Sales (DeptId=6)
('SE',    'Sales Executive',            6, 'L2', 1, '2024-07-01', '2024-07-01'),
('SM',    'Sales Manager',              6, 'L5', 1, '2024-07-01', '2024-07-01'),
('SD',    'Sales Director',             6, 'L6', 1, '2024-07-01', '2024-07-01'),
-- Legal (DeptId=7)
('LA',    'Legal Associate',            7, 'L3', 1, '2024-07-01', '2024-07-01'),
('GC',    'General Counsel',            7, 'L6', 1, '2024-07-01', '2024-07-01'),
-- Payroll (DeptId=8)
('PP',    'Payroll Processor',          8, 'L3', 1, '2024-07-01', '2024-07-01'),
('PPA',   'Payroll Analyst',            8, 'L4', 1, '2024-07-01', '2024-07-01'),
('PPM',   'Payroll Manager',            8, 'L5', 1, '2024-07-01', '2024-07-01');
GO

-- =============================================================
-- STEP 6: USERS (52 users: 1 Admin + 1 HR + 1 PayrollProcessor + 4 Managers + 45 Employees)
-- Password for ALL: EasyPay@123!
-- Hash: $2a$11$hqJV3PIRZMnNX3z9X5.FLeTgRJMV7vOERQ3YCvjK5uVVLxzKF.bGO
-- =============================================================

DECLARE @hash NVARCHAR(500) = '$2a$11$hqJV3PIRZMnNX3z9X5.FLeTgRJMV7vOERQ3YCvjK5uVVLxzKF.bGO';

-- Admin (RoleId=1)
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, IsEmailVerified, FailedLoginAttempts, CreatedAt, UpdatedAt) VALUES
('admin', 'admin@easypay.com', @hash, 1, 1, 1, 0, '2024-07-01', '2024-07-01');

-- HR Manager (RoleId=2) - User's real email
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, IsEmailVerified, FailedLoginAttempts, CreatedAt, UpdatedAt) VALUES
('amitabh.bachchan', 'grootthe38@gmail.com', @hash, 2, 1, 1, 0, '2024-07-01', '2024-07-01');

-- Payroll Processor (RoleId=3)
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, IsEmailVerified, FailedLoginAttempts, CreatedAt, UpdatedAt) VALUES
('rajinikanth.superstar', 'rajinikanth@easypay.in', @hash, 3, 1, 1, 0, '2024-07-01', '2024-07-01');

-- Managers (RoleId=5) - 4 managers, one per major dept
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, IsEmailVerified, FailedLoginAttempts, CreatedAt, UpdatedAt) VALUES
('aamir.khan',       'aamir.khan@easypay.in',       @hash, 5, 1, 1, 0, '2024-07-01', '2024-07-01'),
('deepika.padukone', 'deepika.padukone@easypay.in', @hash, 5, 1, 1, 0, '2024-07-01', '2024-07-01'),
('ranveer.singh',    'ranveer.singh@easypay.in',     @hash, 5, 1, 1, 0, '2024-07-01', '2024-07-01'),
('priyanka.chopra',  'priyanka.chopra@easypay.in',   @hash, 5, 1, 1, 0, '2024-07-01', '2024-07-01');

-- Employees (RoleId=4) - 45 employees with Indian celebrity names
-- Employee #1 is the user's real email
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, IsEmailVerified, FailedLoginAttempts, CreatedAt, UpdatedAt) VALUES
('sugan.prabha',        'suganprabhakaran196@gmail.com',     @hash, 4, 1, 1, 0, '2024-07-15', '2024-07-15'),
('shah.rukh.khan',      'shahrukhkhan@easypay.in',           @hash, 4, 1, 1, 0, '2024-07-15', '2024-07-15'),
('katrina.kaif',        'katrina.kaif@easypay.in',           @hash, 4, 1, 1, 0, '2024-07-15', '2024-07-15'),
('salman.khan',         'salman.khan@easypay.in',            @hash, 4, 1, 1, 0, '2024-07-15', '2024-07-15'),
('kareena.kapoor',      'kareena.kapoor@easypay.in',         @hash, 4, 1, 1, 0, '2024-07-15', '2024-07-15'),
('ranbir.kapoor',       'ranbir.kapoor@easypay.in',          @hash, 4, 1, 1, 0, '2024-07-15', '2024-07-15'),
('alia.bhatt',          'alia.bhatt@easypay.in',             @hash, 4, 1, 1, 0, '2024-07-15', '2024-07-15'),
('varun.dhawan',        'varun.dhawan@easypay.in',           @hash, 4, 1, 1, 0, '2024-08-01', '2024-08-01'),
('anushka.sharma',      'anushka.sharma@easypay.in',         @hash, 4, 1, 1, 0, '2024-08-01', '2024-08-01'),
('hrithik.roshan',      'hrithik.roshan@easypay.in',         @hash, 4, 1, 1, 0, '2024-08-01', '2024-08-01'),
('sonam.kapoor',        'sonam.kapoor@easypay.in',           @hash, 4, 1, 1, 0, '2024-08-01', '2024-08-01'),
('akshay.kumar',        'akshay.kumar@easypay.in',           @hash, 4, 1, 1, 0, '2024-08-15', '2024-08-15'),
('shraddha.kapoor',     'shraddha.kapoor@easypay.in',        @hash, 4, 1, 1, 0, '2024-08-15', '2024-08-15'),
('siddharth.malhotra',  'siddharth.malhotra@easypay.in',     @hash, 4, 1, 1, 0, '2024-08-15', '2024-08-15'),
('madhuri.dixit',       'madhuri.dixit@easypay.in',          @hash, 4, 1, 1, 0, '2024-09-01', '2024-09-01'),
('john.abraham',        'john.abraham@easypay.in',           @hash, 4, 1, 1, 0, '2024-09-01', '2024-09-01'),
('parineeti.chopra',    'parineeti.chopra@easypay.in',       @hash, 4, 1, 1, 0, '2024-09-01', '2024-09-01'),
('tiger.shroff',        'tiger.shroff@easypay.in',           @hash, 4, 1, 1, 0, '2024-09-15', '2024-09-15'),
('jacqueline.fernandez','jacqueline.fernandez@easypay.in',   @hash, 4, 1, 1, 0, '2024-09-15', '2024-09-15'),
('vikram.chandra',      'vikram.chandra@easypay.in',         @hash, 4, 1, 1, 0, '2024-10-01', '2024-10-01'),
('taapsee.pannu',       'taapsee.pannu@easypay.in',          @hash, 4, 1, 1, 0, '2024-10-01', '2024-10-01'),
('prabhas.rebel',       'prabhas.rebel@easypay.in',          @hash, 4, 1, 1, 0, '2024-10-01', '2024-10-01'),
('samantha.ruth',       'samantha.ruth@easypay.in',          @hash, 4, 1, 1, 0, '2024-10-15', '2024-10-15'),
('allu.arjun',          'allu.arjun@easypay.in',             @hash, 4, 1, 1, 0, '2024-10-15', '2024-10-15'),
('rashmika.mandanna',   'rashmika.mandanna@easypay.in',      @hash, 4, 1, 1, 0, '2024-11-01', '2024-11-01'),
('ntr.jr',              'ntr.jr@easypay.in',                 @hash, 4, 1, 1, 0, '2024-11-01', '2024-11-01'),
('vijay.deverakonda',   'vijay.deverakonda@easypay.in',      @hash, 4, 1, 1, 0, '2024-11-01', '2024-11-01'),
('nayanthara.star',     'nayanthara.star@easypay.in',        @hash, 4, 1, 1, 0, '2024-11-15', '2024-11-15'),
('dhanush.kumar',       'dhanush.kumar@easypay.in',          @hash, 4, 1, 1, 0, '2024-11-15', '2024-11-15'),
('kajal.aggarwal',      'kajal.aggarwal@easypay.in',         @hash, 4, 1, 1, 0, '2024-12-01', '2024-12-01'),
('suriya.sivakumar',    'suriya.sivakumar@easypay.in',       @hash, 4, 1, 1, 0, '2024-12-01', '2024-12-01'),
('pooja.hegde',         'pooja.hegde@easypay.in',            @hash, 4, 1, 1, 0, '2024-12-15', '2024-12-15'),
('ram.charan',          'ram.charan@easypay.in',             @hash, 4, 1, 1, 0, '2024-12-15', '2024-12-15'),
('kartik.aaryan',       'kartik.aaryan@easypay.in',          @hash, 4, 1, 1, 0, '2025-01-02', '2025-01-02'),
('kriti.sanon',         'kriti.sanon@easypay.in',            @hash, 4, 1, 1, 0, '2025-01-02', '2025-01-02'),
('ayushmann.khurrana',  'ayushmann.khurrana@easypay.in',     @hash, 4, 1, 1, 0, '2025-01-15', '2025-01-15'),
('bhumi.pednekar',      'bhumi.pednekar@easypay.in',         @hash, 4, 1, 1, 0, '2025-01-15', '2025-01-15'),
('rajkumar.rao',        'rajkumar.rao@easypay.in',           @hash, 4, 1, 1, 0, '2025-02-01', '2025-02-01'),
('janhvi.kapoor',       'janhvi.kapoor@easypay.in',          @hash, 4, 1, 1, 0, '2025-02-01', '2025-02-01'),
('vicky.kaushal',       'vicky.kaushal@easypay.in',          @hash, 4, 1, 1, 0, '2025-02-15', '2025-02-15'),
('sara.ali.khan',       'sara.ali.khan@easypay.in',          @hash, 4, 1, 1, 0, '2025-02-15', '2025-02-15'),
('aditya.roy',          'aditya.roy@easypay.in',             @hash, 4, 1, 1, 0, '2025-03-01', '2025-03-01'),
('kiara.advani',        'kiara.advani@easypay.in',           @hash, 4, 1, 1, 0, '2025-03-01', '2025-03-01'),
('sonu.sood',           'sonu.sood@easypay.in',              @hash, 4, 1, 1, 0, '2025-03-15', '2025-03-15'),
('tamannaah.bhatia',    'tamannaah.bhatia@easypay.in',       @hash, 4, 1, 1, 0, '2025-04-01', '2025-04-01');
GO

-- =============================================================
-- STEP 7: EMPLOYEES (linked to users)
-- UserIds: Admin=1, HRMgr=2, PayProc=3, Mgr1=4,Mgr2=5,Mgr3=6,Mgr4=7
-- Employees start at UserId=8
-- =============================================================

-- HR Manager employee record (UserId=2, DeptId=2/HR, DesignId=8/HRM)
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0001',2,'Amitabh','Bachchan','1942-10-11','Male','AABB1234567','9876543210','grootthe38@gmail.com',
 '10 Jalsa Estate, Juhu','Mumbai','Maharashtra','400049','India',2,8,NULL,
 '2024-07-01','2024-10-01','FullTime','Active',
 'HDFC Bank','HD1234567890','HDFC0001234','AAABB1234A','PF001234','ESI001234',10.0,1,
 '2024-07-01','2024-07-01',1);

-- Payroll Processor employee record (UserId=3, DeptId=8/Payroll, DesignId=29/PP)
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0002',3,'Rajinikanth','Superstar','1950-12-12','Male','ARRS5678901','9988776655','rajinikanth@easypay.in',
 '5 Poes Garden','Chennai','Tamil Nadu','600086','India',8,27,1,
 '2024-07-01','2024-10-01','FullTime','Active',
 'SBI','SB5678901234','SBIN0001234','BRRRS5678A','PF005678','ESI005678',10.0,1,
 '2024-07-01','2024-07-01',1);

-- Manager 1 - Aamir Khan - IT (UserId=4, DeptId=1, DesignId=3/TL)
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0003',4,'Aamir','Khan','1965-03-14','Male','AAKK9012345','9123456789','aamir.khan@easypay.in',
 '42 Hill Road, Bandra West','Mumbai','Maharashtra','400050','India',1,3,1,
 '2024-07-01','2024-10-01','FullTime','Active',
 'ICICI Bank','IC9012345678','ICIC0001234','CAAAK9012A','PF009012','ESI009012',15.0,1,
 '2024-07-01','2024-07-01',1);

-- Manager 2 - Deepika Padukone - Marketing (UserId=5, DeptId=5, DesignId=20/MM)
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0004',5,'Deepika','Padukone','1986-01-05','Female','ADPP3456789','9234567890','deepika.padukone@easypay.in',
 '15 Bandstand, Bandra','Mumbai','Maharashtra','400050','India',5,20,1,
 '2024-07-01','2024-10-01','FullTime','Active',
 'Axis Bank','AX3456789012','UTIB0001234','DDDPA3456A','PF003456','ESI003456',15.0,1,
 '2024-07-01','2024-07-01',1);

-- Manager 3 - Ranveer Singh - Sales (UserId=6, DeptId=6, DesignId=23/SM)
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0005',6,'Ranveer','Singh','1985-07-06','Male','ARSS7890123','9345678901','ranveer.singh@easypay.in',
 '8 Lokhandwala Complex, Andheri West','Mumbai','Maharashtra','400053','India',6,23,1,
 '2024-07-01','2024-10-01','FullTime','Active',
 'Kotak Mahindra','KM7890123456','KKBK0001234','ERRRS7890A','PF007890','ESI007890',15.0,1,
 '2024-07-01','2024-07-01',1);

-- Manager 4 - Priyanka Chopra - Operations (UserId=7, DeptId=4, DesignId=16/OM)
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0006',7,'Priyanka','Chopra','1982-07-18','Female','APCC2345678','9456789012','priyanka.chopra@easypay.in',
 '25 Versova, Andheri West','Mumbai','Maharashtra','400061','India',4,16,1,
 '2024-07-01','2024-10-01','FullTime','Active',
 'Yes Bank','YB2345678901','YESB0001234','FPPCA2345A','PF002345','ESI002345',15.0,1,
 '2024-07-01','2024-07-01',1);

-- Employees 7-51 (UserId 8-52), 45 employees
-- Sugan Prabha (UserId=8) - real email user
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0007',8,'Sugan','Prabha','1998-06-15','Male','ASSP4561230','8765432109','suganprabhakaran196@gmail.com',
 '12 Anna Nagar East','Chennai','Tamil Nadu','600102','India',1,1,3,
 '2024-07-15','2024-10-15','FullTime','Active',
 'SBI','SB4561230987','SBIN0002345','GSSPA4561A','PF004561','ESI004561',0.0,1,
 '2024-07-15','2024-07-15',1);

-- Shah Rukh Khan (UserId=9) - IT SWE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0008',9,'Shah Rukh','Khan','1965-11-02','Male','ASRK1230987','9567890123','shahrukhkhan@easypay.in',
 '1 Mannat, Bandstand, Bandra West','Mumbai','Maharashtra','400050','India',1,1,3,
 '2024-07-15','2024-10-15','FullTime','Active',
 'HDFC Bank','HD1230987654','HDFC0002345','HSRKK1230A','PF001230','ESI001230',5.0,1,
 '2024-07-15','2024-07-15',1);

-- Katrina Kaif (UserId=10) - Marketing Executive
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0009',10,'Katrina','Kaif','1984-07-16','Female','AKKK7894560','9678901234','katrina.kaif@easypay.in',
 '8 Sea View, Bandra','Mumbai','Maharashtra','400050','India',5,18,4,
 '2024-07-15','2024-10-15','FullTime','Active',
 'ICICI Bank','IC7894560123','ICIC0002345','IKKKA7894A','PF007894','ESI007894',5.0,1,
 '2024-07-15','2024-07-15',1);

-- Salman Khan (UserId=11) - Sales Executive
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0010',11,'Salman','Khan','1965-12-27','Male','ASKK3217890','9789012345','salman.khan@easypay.in',
 'Galaxy Apartments, Bandra West','Mumbai','Maharashtra','400050','India',6,22,5,
 '2024-07-15','2024-10-15','FullTime','Active',
 'Axis Bank','AX3217890456','UTIB0002345','JSKKK3217A','PF003217','ESI003217',5.0,1,
 '2024-07-15','2024-07-15',1);

-- Kareena Kapoor (UserId=12) - HR Specialist
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0011',12,'Kareena','Kapoor','1980-09-21','Female','AKKK6540321','9890123456','kareena.kapoor@easypay.in',
 '4 Forbes Street, Fort','Mumbai','Maharashtra','400001','India',2,7,1,
 '2024-07-15','2024-10-15','FullTime','Active',
 'SBI','SB6540321789','SBIN0003456','KKKKA6540A','PF006540','ESI006540',5.0,1,
 '2024-07-15','2024-07-15',1);

-- Ranbir Kapoor (UserId=13) - IT SSE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0012',13,'Ranbir','Kapoor','1982-09-28','Male','ARKK2109876','8901234567','ranbir.kapoor@easypay.in',
 '3 Krishna Raj Building, Pali Hill','Mumbai','Maharashtra','400050','India',1,2,3,
 '2024-07-15','2024-10-15','FullTime','Active',
 'HDFC Bank','HD2109876543','HDFC0003456','LRKKK2109A','PF002109','ESI002109',5.0,1,
 '2024-07-15','2024-07-15',1);

-- Alia Bhatt (UserId=14) - Finance Analyst
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0013',14,'Alia','Bhatt','1993-03-15','Female','AABB8765432','7012345678','alia.bhatt@easypay.in',
 '6 Juhu Tara Road','Mumbai','Maharashtra','400049','India',3,11,1,
 '2024-07-15','2024-10-15','FullTime','Active',
 'ICICI Bank','IC8765432109','ICIC0003456','MAABB8765A','PF008765','ESI008765',5.0,1,
 '2024-07-15','2024-07-15',1);

-- Varun Dhawan (UserId=15) - IT SWE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0014',15,'Varun','Dhawan','1987-04-24','Male','AVDD4321098','8123456789','varun.dhawan@easypay.in',
 '14 Lokhandwala, Andheri','Mumbai','Maharashtra','400053','India',1,1,3,
 '2024-08-01','2024-11-01','FullTime','Active',
 'Axis Bank','AX4321098765','UTIB0003456','NVDDA4321A','PF004321','ESI004321',0.0,1,
 '2024-08-01','2024-08-01',1);

-- Anushka Sharma (UserId=16) - HR Executive
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0015',16,'Anushka','Sharma','1988-05-01','Female','AASS6543210','8234567890','anushka.sharma@easypay.in',
 '12 Yari Road, Versova','Mumbai','Maharashtra','400061','India',2,6,1,
 '2024-08-01','2024-11-01','FullTime','Active',
 'SBI','SB6543210987','SBIN0004567','OASSA6543A','PF006543','ESI006543',0.0,1,
 '2024-08-01','2024-08-01',1);

-- Hrithik Roshan (UserId=17) - IT Tech Lead
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0016',17,'Hrithik','Roshan','1974-01-10','Male','AHRR8901234','8345678901','hrithik.roshan@easypay.in',
 '22 Bandra Hills, Bandra West','Mumbai','Maharashtra','400050','India',1,3,3,
 '2024-08-01','2024-11-01','FullTime','Active',
 'HDFC Bank','HD8901234567','HDFC0004567','PHRRA8901A','PF008901','ESI008901',10.0,1,
 '2024-08-01','2024-08-01',1);

-- Sonam Kapoor (UserId=18) - Finance Analyst
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0017',18,'Sonam','Kapoor','1985-06-09','Female','ASKK0123456','8456789012','sonam.kapoor@easypay.in',
 '35 Lodhi Road Colony','New Delhi','Delhi','110003','India',3,11,1,
 '2024-08-01','2024-11-01','FullTime','Active',
 'ICICI Bank','IC0123456789','ICIC0004567','QSKKA0123A','PF000123','ESI000123',5.0,1,
 '2024-08-01','2024-08-01',1);

-- Akshay Kumar (UserId=19) - Operations Analyst
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0018',19,'Akshay','Kumar','1967-09-09','Male','AAKK2345678','8567890123','akshay.kumar@easypay.in',
 '17 Shivaji Park, Dadar','Mumbai','Maharashtra','400028','India',4,15,6,
 '2024-08-15','2024-11-15','FullTime','Active',
 'Axis Bank','AX2345678901','UTIB0004567','RAKKK2345A','PF002345','ESI002345',5.0,1,
 '2024-08-15','2024-08-15',1);

-- Shraddha Kapoor (UserId=20) - Marketing Specialist
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0019',20,'Shraddha','Kapoor','1989-03-03','Female','ASKK4567890','8678901234','shraddha.kapoor@easypay.in',
 '9 Walkeshwar Road, Malabar Hill','Mumbai','Maharashtra','400006','India',5,19,4,
 '2024-08-15','2024-11-15','FullTime','Active',
 'SBI','SB4567890123','SBIN0005678','SKKKA4567A','PF004567','ESI004567',0.0,1,
 '2024-08-15','2024-08-15',1);

-- Siddharth Malhotra (UserId=21) - Sales Executive
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0020',21,'Siddharth','Malhotra','1985-01-16','Male','ASM6789012','8789012345','siddharth.malhotra@easypay.in',
 '23 Versova, Andheri West','Mumbai','Maharashtra','400061','India',6,22,5,
 '2024-08-15','2024-11-15','FullTime','Active',
 'HDFC Bank','HD6789012345','HDFC0005678','TSMMA6789A','PF006789','ESI006789',0.0,1,
 '2024-08-15','2024-08-15',1);

-- Madhuri Dixit (UserId=22) - HR Manager (reports to HRMgr EmpId=1)
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0021',22,'Madhuri','Dixit','1967-05-15','Female','AMDD8901234','8890123456','madhuri.dixit@easypay.in',
 '7 Linking Road, Bandra West','Mumbai','Maharashtra','400050','India',2,8,1,
 '2024-09-01','2024-12-01','FullTime','Active',
 'ICICI Bank','IC8901234567','ICIC0005678','UMDDA8901A','PF008901X','ESI008901',10.0,1,
 '2024-09-01','2024-09-01',1);

-- John Abraham (UserId=23) - IT SSE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0022',23,'John','Abraham','1972-12-17','Male','AJAA0123456','8901234567','john.abraham@easypay.in',
 '50 Khar West, Mumbai','Mumbai','Maharashtra','400052','India',1,2,3,
 '2024-09-01','2024-12-01','FullTime','Active',
 'Axis Bank','AX0123456789','UTIB0005678','VJAAA0123A','PF000123X','ESI000123',5.0,1,
 '2024-09-01','2024-09-01',1);

-- Parineeti Chopra (UserId=24) - Finance Analyst
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0023',24,'Parineeti','Chopra','1988-10-22','Female','APCC2222222','7890123456','parineeti.chopra@easypay.in',
 '18 Colaba Causeway','Mumbai','Maharashtra','400005','India',3,11,1,
 '2024-09-01','2024-12-01','FullTime','Active',
 'SBI','SB2222222345','SBIN0006789','WPCCA2222A','PF002222','ESI002222',0.0,1,
 '2024-09-01','2024-09-01',1);

-- Tiger Shroff (UserId=25) - IT SWE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0024',25,'Tiger','Shroff','1990-03-02','Male','ATSS3333333','7901234567','tiger.shroff@easypay.in',
 '30 Oshiwara, Andheri West','Mumbai','Maharashtra','400053','India',1,1,3,
 '2024-09-15','2024-12-15','FullTime','Active',
 'HDFC Bank','HD3333333456','HDFC0006789','XTSSA3333A','PF003333','ESI003333',0.0,1,
 '2024-09-15','2024-09-15',1);

-- Jacqueline Fernandez (UserId=26) - Marketing Executive
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0025',26,'Jacqueline','Fernandez','1985-08-11','Female','AJFF4444444','7012345679','jacqueline.fernandez@easypay.in',
 '11 Juhu Beach Road','Mumbai','Maharashtra','400049','India',5,18,4,
 '2024-09-15','2024-12-15','FullTime','Active',
 'ICICI Bank','IC4444444567','ICIC0006789','YJFFA4444A','PF004444','ESI004444',0.0,1,
 '2024-09-15','2024-09-15',1);

-- Vikram Chandra (UserId=27) - Payroll Analyst
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0026',27,'Vikram','Chandra','1975-12-01','Male','AVCC5555555','7123456780','vikram.chandra@easypay.in',
 '44 T. Nagar','Chennai','Tamil Nadu','600017','India',8,28,2,
 '2024-10-01','2025-01-01','FullTime','Active',
 'Axis Bank','AX5555555678','UTIB0006789','ZVCCA5555A','PF005555','ESI005555',0.0,1,
 '2024-10-01','2024-10-01',1);

-- Taapsee Pannu (UserId=28) - IT SWE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0027',28,'Taapsee','Pannu','1987-08-01','Female','ATPP6666666','7234567891','taapsee.pannu@easypay.in',
 '22 Punjabi Bagh','New Delhi','Delhi','110026','India',1,1,3,
 '2024-10-01','2025-01-01','FullTime','Active',
 'SBI','SB6666666789','SBIN0007890','ATPPA6666A','PF006666','ESI006666',0.0,1,
 '2024-10-01','2024-10-01',1);

-- Prabhas (UserId=29) - IT Tech Lead
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0028',29,'Prabhas','Rebel','1979-10-23','Male','APRR7777777','7345678902','prabhas.rebel@easypay.in',
 '5 Film Nagar, Jubilee Hills','Hyderabad','Telangana','500033','India',1,3,3,
 '2024-10-01','2025-01-01','FullTime','Active',
 'HDFC Bank','HD7777777890','HDFC0007890','BPRRA7777A','PF007777','ESI007777',10.0,1,
 '2024-10-01','2024-10-01',1);

-- Samantha Ruth (UserId=30) - Marketing Specialist
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0029',30,'Samantha','Ruth','1987-04-28','Female','ASRR8888888','7456789013','samantha.ruth@easypay.in',
 '12 Jubilee Hills Road 45','Hyderabad','Telangana','500033','India',5,19,4,
 '2024-10-15','2025-01-15','FullTime','Active',
 'ICICI Bank','IC8888888901','ICIC0007890','CSRRA8888A','PF008888','ESI008888',0.0,1,
 '2024-10-15','2024-10-15',1);

-- Allu Arjun (UserId=31) - Sales Executive
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0030',31,'Allu','Arjun','1982-04-08','Male','AAAA9999999','7567890124','allu.arjun@easypay.in',
 '3 Filmnagar, Banjara Hills','Hyderabad','Telangana','500034','India',6,22,5,
 '2024-10-15','2025-01-15','FullTime','Active',
 'Axis Bank','AX9999999012','UTIB0007890','DAAAA9999A','PF009999','ESI009999',0.0,1,
 '2024-10-15','2024-10-15',1);

-- Rashmika Mandanna (UserId=32) - Operations Analyst
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0031',32,'Rashmika','Mandanna','1996-04-05','Female','ARMM1111111','7678901235','rashmika.mandanna@easypay.in',
 '28 MG Road, Virajpet','Bengaluru','Karnataka','560001','India',4,15,6,
 '2024-11-01','2025-02-01','FullTime','Active',
 'SBI','SB1111111234','SBIN0008901','ERMMA1111A','PF001111','ESI001111',0.0,1,
 '2024-11-01','2024-11-01',1);

-- NTR Jr (UserId=33) - IT SSE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0032',33,'NTR','Jr','1983-05-20','Male','ANTT2222222','7789012346','ntr.jr@easypay.in',
 '10 Road No 12, Banjara Hills','Hyderabad','Telangana','500034','India',1,2,3,
 '2024-11-01','2025-02-01','FullTime','Active',
 'HDFC Bank','HD2222222345','HDFC0008901','FNTTR2222A','PF002222X','ESI002222',5.0,1,
 '2024-11-01','2024-11-01',1);

-- Vijay Deverakonda (UserId=34) - Sales Manager
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0033',34,'Vijay','Deverakonda','1989-05-09','Male','AVDD3333333','7890123457','vijay.deverakonda@easypay.in',
 '18 Jubilee Hills Road 10','Hyderabad','Telangana','500033','India',6,23,5,
 '2024-11-01','2025-02-01','FullTime','Active',
 'ICICI Bank','IC3333333456','ICIC0008901','GVDDA3333A','PF003333X','ESI003333',10.0,1,
 '2024-11-01','2024-11-01',1);

-- Nayanthara (UserId=35) - HR Business Partner
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0034',35,'Nayanthara','Star','1984-11-18','Female','ANSS4444444','7901234568','nayanthara.star@easypay.in',
 '7 Besant Nagar','Chennai','Tamil Nadu','600090','India',2,9,1,
 '2024-11-15','2025-02-15','FullTime','Active',
 'Axis Bank','AX4444444567','UTIB0008901','HNSSA4444A','PF004444X','ESI004444',5.0,1,
 '2024-11-15','2024-11-15',1);

-- Dhanush (UserId=36) - IT SWE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0035',36,'Dhanush','Kumar','1983-07-28','Male','ADKK5555555','7012345680','dhanush.kumar@easypay.in',
 '15 Karpagam Avenue, R A Puram','Chennai','Tamil Nadu','600028','India',1,1,3,
 '2024-11-15','2025-02-15','FullTime','Active',
 'SBI','SB5555555678','SBIN0009012','IDKKA5555A','PF005555X','ESI005555',0.0,1,
 '2024-11-15','2024-11-15',1);

-- Kajal Aggarwal (UserId=37) - Finance Manager
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0036',37,'Kajal','Aggarwal','1985-06-19','Female','AKAA6666666','7123456781','kajal.aggarwal@easypay.in',
 '9 Pedamma Temple Road, Jubilee Hills','Hyderabad','Telangana','500033','India',3,13,1,
 '2024-12-01','2025-03-01','FullTime','Active',
 'HDFC Bank','HD6666666789','HDFC0009012','JKAAA6666A','PF006666X','ESI006666',10.0,1,
 '2024-12-01','2024-12-01',1);

-- Suriya (UserId=38) - Operations Manager
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0037',38,'Suriya','Sivakumar','1975-07-23','Male','ASSS7777777','7234567892','suriya.sivakumar@easypay.in',
 '42 Poes Garden, Teynampet','Chennai','Tamil Nadu','600086','India',4,16,6,
 '2024-12-01','2025-03-01','FullTime','Active',
 'ICICI Bank','IC7777777890','ICIC0009012','KSSSS7777A','PF007777X','ESI007777',10.0,1,
 '2024-12-01','2024-12-01',1);

-- Pooja Hegde (UserId=39) - HR Executive
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0038',39,'Pooja','Hegde','1990-10-13','Female','APHH8888888','7345678903','pooja.hegde@easypay.in',
 '6 Mangaluru Road, Balmatta','Mangaluru','Karnataka','575001','India',2,6,1,
 '2024-12-15','2025-03-15','FullTime','Active',
 'Axis Bank','AX8888888901','UTIB0009012','LPHHA8888A','PF008888X','ESI008888',0.0,1,
 '2024-12-15','2024-12-15',1);

-- Ram Charan (UserId=40) - IT SSE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0039',40,'Ram','Charan','1985-03-27','Male','ARCC9999999','7456789014','ram.charan@easypay.in',
 '1 Road No 2, Banjara Hills','Hyderabad','Telangana','500034','India',1,2,3,
 '2024-12-15','2025-03-15','FullTime','Active',
 'SBI','SB9999999012','SBIN0010123','MRCCR9999A','PF009999X','ESI009999',5.0,1,
 '2024-12-15','2024-12-15',1);

-- Kartik Aaryan (UserId=41) - Marketing Specialist
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0040',41,'Kartik','Aaryan','1990-11-22','Male','AKAA0000000','7567890125','kartik.aaryan@easypay.in',
 '13 Mahalakshmi Nagar, Gwalior','Gwalior','Madhya Pradesh','474012','India',5,19,4,
 '2025-01-02','2025-04-02','FullTime','Active',
 'HDFC Bank','HD0000000123','HDFC0010123','NKAAR0000A','PF000000','ESI000000',0.0,1,
 '2025-01-02','2025-01-02',1);

-- Kriti Sanon (UserId=42) - Legal Associate
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0041',42,'Kriti','Sanon','1990-07-27','Female','AKSS1111110','7678901236','kriti.sanon@easypay.in',
 '21 Saket, New Delhi','New Delhi','Delhi','110017','India',7,25,1,
 '2025-01-02','2025-04-02','FullTime','Active',
 'ICICI Bank','IC1111110234','ICIC0010123','OKSSS1111A','PF001110','ESI001110',0.0,1,
 '2025-01-02','2025-01-02',1);

-- Ayushmann Khurrana (UserId=43) - IT SWE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0042',43,'Ayushmann','Khurrana','1984-09-14','Male','AAKK2222220','7789012347','ayushmann.khurrana@easypay.in',
 '8 Sector 7, Chandigarh','Chandigarh','Chandigarh','160019','India',1,1,3,
 '2025-01-15','2025-04-15','FullTime','Active',
 'Axis Bank','AX2222220345','UTIB0010123','PAKKK2222A','PF002220','ESI002220',0.0,1,
 '2025-01-15','2025-01-15',1);

-- Bhumi Pednekar (UserId=44) - Finance Analyst
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0043',44,'Bhumi','Pednekar','1989-07-18','Female','ABPP3333330','7890123458','bhumi.pednekar@easypay.in',
 '5 Pedder Road, Breach Candy','Mumbai','Maharashtra','400026','India',3,11,1,
 '2025-01-15','2025-04-15','FullTime','Active',
 'SBI','SB3333330456','SBIN0011234','QBPPA3333A','PF003330','ESI003330',0.0,1,
 '2025-01-15','2025-01-15',1);

-- Rajkumar Rao (UserId=45) - IT SSE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0044',45,'Rajkumar','Rao','1984-08-31','Male','ARRR4444440','7901234569','rajkumar.rao@easypay.in',
 '17 Gurgaon DLF Phase 1','Gurugram','Haryana','122001','India',1,2,3,
 '2025-02-01','2025-05-01','FullTime','Active',
 'HDFC Bank','HD4444440567','HDFC0011234','RRRRA4444A','PF004440','ESI004440',0.0,1,
 '2025-02-01','2025-02-01',1);

-- Janhvi Kapoor (UserId=46) - Operations Analyst
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0045',46,'Janhvi','Kapoor','1997-03-06','Female','AJKK5555550','7012345681','janhvi.kapoor@easypay.in',
 '18 Chembur Colony','Mumbai','Maharashtra','400071','India',4,15,6,
 '2025-02-01','2025-05-01','FullTime','Active',
 'ICICI Bank','IC5555550678','ICIC0011234','SJKKR5555A','PF005550','ESI005550',0.0,1,
 '2025-02-01','2025-02-01',1);

-- Vicky Kaushal (UserId=47) - IT SWE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0046',47,'Vicky','Kaushal','1988-05-16','Male','AVKK6666660','7123456782','vicky.kaushal@easypay.in',
 '20 Versova Village, Andheri','Mumbai','Maharashtra','400061','India',1,1,3,
 '2025-02-15','2025-05-15','FullTime','Active',
 'Axis Bank','AX6666660789','UTIB0011234','TVKKA6666A','PF006660','ESI006660',0.0,1,
 '2025-02-15','2025-02-15',1);

-- Sara Ali Khan (UserId=48) - Marketing Executive
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0047',48,'Sara Ali','Khan','1995-08-12','Female','ASKK7777770','7234567893','sara.ali.khan@easypay.in',
 '16 Pali Hill, Bandra West','Mumbai','Maharashtra','400050','India',5,18,4,
 '2025-02-15','2025-05-15','FullTime','Active',
 'SBI','SB7777770890','SBIN0012345','USKKA7777A','PF007770','ESI007770',0.0,1,
 '2025-02-15','2025-02-15',1);

-- Aditya Roy (UserId=49) - Payroll Processor
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0048',49,'Aditya','Roy','1985-11-16','Male','AARR8888880','7345678904','aditya.roy@easypay.in',
 '11 New Alipore, Block D','Kolkata','West Bengal','700053','India',8,27,2,
 '2025-03-01','2025-06-01','FullTime','Active',
 'HDFC Bank','HD8888880901','HDFC0012345','VARRR8888A','PF008880','ESI008880',0.0,1,
 '2025-03-01','2025-03-01',1);

-- Kiara Advani (UserId=50) - Sales Executive
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0049',50,'Kiara','Advani','1992-07-31','Female','AKAA9999990','7456789015','kiara.advani@easypay.in',
 '3 Colaba, Near Gateway','Mumbai','Maharashtra','400005','India',6,22,5,
 '2025-03-01','2025-06-01','FullTime','Active',
 'ICICI Bank','IC9999990012','ICIC0012345','WKAAA9999A','PF009990','ESI009990',0.0,1,
 '2025-03-01','2025-03-01',1);

-- Sonu Sood (UserId=51) - General Counsel Legal
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0050',51,'Sonu','Sood','1973-07-30','Male','ASSS0000001','7567890126','sonu.sood@easypay.in',
 '14 Moga Road, Amritsar','Amritsar','Punjab','143001','India',7,26,1,
 '2025-03-15','2025-06-15','FullTime','Active',
 'Axis Bank','AX0000001234','UTIB0012345','XSSSO0000A','PF000001','ESI000001',10.0,1,
 '2025-03-15','2025-03-15',1);

-- Tamannaah Bhatia (UserId=52) - IT SWE
INSERT INTO Employees (EmployeeCode,UserId,FirstName,LastName,DateOfBirth,Gender,NationalId,Phone,PersonalEmail,
    Address,City,State,PostalCode,Country,DepartmentId,DesignationId,ManagerId,
    JoiningDate,ConfirmationDate,EmploymentType,EmploymentStatus,
    BankName,BankAccountNo,BankIFSC,PanNumber,PfNumber,EsiNumber,TaxWithholding,IsActive,
    CreatedAt,UpdatedAt,CreatedBy)
VALUES
('EMP0051',52,'Tamannaah','Bhatia','1989-12-21','Female','ATBB1111112','7678901237','tamannaah.bhatia@easypay.in',
 '19 Jubilee Hills Road 36','Hyderabad','Telangana','500033','India',1,1,3,
 '2025-04-01','2025-07-01','FullTime','Active',
 'SBI','SB1111112345','SBIN0013456','YTBBB1111A','PF001112','ESI001112',0.0,1,
 '2025-04-01','2025-04-01',1);

GO

-- =============================================================
-- STEP 8: UPDATE DEPARTMENT MANAGER USER IDs
-- =============================================================
UPDATE Departments SET ManagerUserId = 2  WHERE DepartmentCode = 'HR';    -- Amitabh HR Mgr
UPDATE Departments SET ManagerUserId = 4  WHERE DepartmentCode = 'IT';    -- Aamir Khan
UPDATE Departments SET ManagerUserId = 5  WHERE DepartmentCode = 'MKT';   -- Deepika
UPDATE Departments SET ManagerUserId = 6  WHERE DepartmentCode = 'SAL';   -- Ranveer
UPDATE Departments SET ManagerUserId = 7  WHERE DepartmentCode = 'OPS';   -- Priyanka
UPDATE Departments SET ManagerUserId = 2  WHERE DepartmentCode = 'FIN';   -- HRMgr covers Finance oversight
UPDATE Departments SET ManagerUserId = 2  WHERE DepartmentCode = 'LEGAL'; -- HRMgr covers Legal
UPDATE Departments SET ManagerUserId = 3  WHERE DepartmentCode = 'PAY';   -- Rajinikanth payroll
GO

-- =============================================================
-- STEP 9: BENEFITS (7 company benefits)
-- =============================================================
INSERT INTO Benefits (BenefitName, BenefitCode, BenefitType, Amount, IsPercentage, Description, IsActive, CreatedAt, UpdatedAt) VALUES
('Health Insurance',           'HI',    'Health',      1500.00, 0, 'Group health insurance for employee and family', 1, '2024-07-01', '2024-07-01'),
('Term Life Insurance',        'TLI',   'Insurance',   500.00,  0, 'Group term life insurance coverage',            1, '2024-07-01', '2024-07-01'),
('Provident Fund Employer',    'PFE',   'Retirement',  12.00,   1, 'Employer PF contribution at 12% of basic',      1, '2024-07-01', '2024-07-01'),
('Transport Allowance',        'TA',    'Transport',   1600.00, 0, 'Monthly transport/conveyance allowance',        1, '2024-07-01', '2024-07-01'),
('Meal Coupon',                'MC',    'Meal',        1100.00, 0, 'Monthly meal voucher/coupon benefit',           1, '2024-07-01', '2024-07-01'),
('Employee Stock Option Plan', 'ESOP',  'Retirement',  5.00,    1, 'ESOP at 5% of CTC for senior employees',       1, '2024-07-01', '2024-07-01'),
('Performance Bonus Pool',     'PBP',   'Health',      10.00,   1, 'Annual performance bonus pool at 10% of CTC',  1, '2024-07-01', '2024-07-01');
GO

-- =============================================================
-- STEP 10: SALARY STRUCTURES
-- All employees get salary structure from their joining date
-- Salary bands by role/designation level:
--   Admin/HR/Payroll Manager: 80000-120000 basic
--   Managers: 70000-100000 basic
--   Sr Engineers / Senior roles: 55000-80000 basic
--   Engineers / Analysts: 35000-55000 basic
--   Executives / Junior: 22000-35000 basic
-- =============================================================

-- PolicyId=1 = FY2024-25, PolicyId=2 = FY2025-26

-- EmpId=1: Amitabh Bachchan (HR Manager)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (1,1,'2024-07-01','2025-06-30',90000,36000,1600,1250,15000,5000,2000,0,'2024-07-01','2024-07-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (1,2,'2025-07-01',NULL,98000,39200,1600,1250,17000,5500,2000,1,'2025-07-01','2025-07-01',1);

-- EmpId=2: Rajinikanth (Payroll Processor)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (2,1,'2024-07-01','2025-06-30',75000,30000,1600,1250,12000,4000,1500,0,'2024-07-01','2024-07-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (2,2,'2025-07-01',NULL,82000,32800,1600,1250,13500,4500,1500,1,'2025-07-01','2025-07-01',1);

-- EmpId=3: Aamir Khan (IT TL/Manager)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (3,1,'2024-07-01','2025-06-30',85000,34000,1600,1250,14000,5000,2000,0,'2024-07-01','2024-07-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (3,2,'2025-07-01',NULL,93000,37200,1600,1250,15500,5500,2000,1,'2025-07-01','2025-07-01',1);

-- EmpId=4: Deepika Padukone (Marketing Manager)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (4,1,'2024-07-01','2025-06-30',80000,32000,1600,1250,13000,4500,1800,0,'2024-07-01','2024-07-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (4,2,'2025-07-01',NULL,88000,35200,1600,1250,14500,5000,1800,1,'2025-07-01','2025-07-01',1);

-- EmpId=5: Ranveer Singh (Sales Manager)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (5,1,'2024-07-01','2025-06-30',80000,32000,1600,1250,13500,4500,1800,0,'2024-07-01','2024-07-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (5,2,'2025-07-01',NULL,88000,35200,1600,1250,14800,5000,1800,1,'2025-07-01','2025-07-01',1);

-- EmpId=6: Priyanka Chopra (Operations Manager)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (6,1,'2024-07-01','2025-06-30',82000,32800,1600,1250,13500,4500,1800,0,'2024-07-01','2024-07-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (6,2,'2025-07-01',NULL,90000,36000,1600,1250,15000,5000,1800,1,'2025-07-01','2025-07-01',1);

-- EmpId=7: Sugan Prabha (SWE - real email user)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (7,1,'2024-07-15','2025-06-30',38000,15200,1600,1250,5000,2000,800,0,'2024-07-15','2024-07-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (7,2,'2025-07-01',NULL,42000,16800,1600,1250,5500,2200,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=8: Shah Rukh Khan (SWE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (8,1,'2024-07-15','2025-06-30',45000,18000,1600,1250,6500,2500,1000,0,'2024-07-15','2024-07-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (8,2,'2025-07-01',NULL,50000,20000,1600,1250,7200,2800,1000,1,'2025-07-01','2025-07-01',1);

-- EmpId=9: Katrina Kaif (Marketing Executive)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (9,1,'2024-07-15','2025-06-30',28000,11200,1600,1250,3800,1500,600,0,'2024-07-15','2024-07-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (9,2,'2025-07-01',NULL,31000,12400,1600,1250,4200,1700,600,1,'2025-07-01','2025-07-01',1);

-- EmpId=10: Salman Khan (Sales Executive)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (10,1,'2024-07-15','2025-06-30',32000,12800,1600,1250,4500,2000,700,0,'2024-07-15','2024-07-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (10,2,'2025-07-01',NULL,35500,14200,1600,1250,5000,2200,700,1,'2025-07-01','2025-07-01',1);

-- EmpId=11: Kareena Kapoor (HR Specialist)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (11,1,'2024-07-15','2025-06-30',40000,16000,1600,1250,5500,2200,900,0,'2024-07-15','2024-07-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (11,2,'2025-07-01',NULL,44000,17600,1600,1250,6000,2400,900,1,'2025-07-01','2025-07-01',1);

-- EmpId=12: Ranbir Kapoor (IT SSE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (12,1,'2024-07-15','2025-06-30',60000,24000,1600,1250,9000,3500,1200,0,'2024-07-15','2024-07-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (12,2,'2025-07-01',NULL,66000,26400,1600,1250,10000,3900,1200,1,'2025-07-01','2025-07-01',1);

-- EmpId=13: Alia Bhatt (Finance Analyst)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (13,1,'2024-07-15','2025-06-30',42000,16800,1600,1250,6000,2500,900,0,'2024-07-15','2024-07-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (13,2,'2025-07-01',NULL,46500,18600,1600,1250,6600,2700,900,1,'2025-07-01','2025-07-01',1);

-- EmpId=14: Varun Dhawan (IT SWE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (14,1,'2024-08-01','2025-06-30',36000,14400,1600,1250,4800,2000,800,0,'2024-08-01','2024-08-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (14,2,'2025-07-01',NULL,40000,16000,1600,1250,5300,2200,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=15: Anushka Sharma (HR Executive)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (15,1,'2024-08-01','2025-06-30',30000,12000,1600,1250,4000,1800,700,0,'2024-08-01','2024-08-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (15,2,'2025-07-01',NULL,33000,13200,1600,1250,4400,2000,700,1,'2025-07-01','2025-07-01',1);

-- EmpId=16: Hrithik Roshan (IT TL)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (16,1,'2024-08-01','2025-06-30',72000,28800,1600,1250,11000,4000,1500,0,'2024-08-01','2024-08-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (16,2,'2025-07-01',NULL,79000,31600,1600,1250,12000,4400,1500,1,'2025-07-01','2025-07-01',1);

-- EmpId=17: Sonam Kapoor (Finance Analyst)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (17,1,'2024-08-01','2025-06-30',40000,16000,1600,1250,5500,2200,800,0,'2024-08-01','2024-08-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (17,2,'2025-07-01',NULL,44000,17600,1600,1250,6000,2400,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=18: Akshay Kumar (Operations Analyst)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (18,1,'2024-08-15','2025-06-30',38000,15200,1600,1250,5000,2000,800,0,'2024-08-15','2024-08-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (18,2,'2025-07-01',NULL,42000,16800,1600,1250,5500,2200,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=19: Shraddha Kapoor (Marketing Specialist)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (19,1,'2024-08-15','2025-06-30',35000,14000,1600,1250,4600,1900,750,0,'2024-08-15','2024-08-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (19,2,'2025-07-01',NULL,38500,15400,1600,1250,5000,2100,750,1,'2025-07-01','2025-07-01',1);

-- EmpId=20: Siddharth Malhotra (Sales Executive)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (20,1,'2024-08-15','2025-06-30',30000,12000,1600,1250,4000,1700,650,0,'2024-08-15','2024-08-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (20,2,'2025-07-01',NULL,33000,13200,1600,1250,4400,1900,650,1,'2025-07-01','2025-07-01',1);

-- EmpId=21: Madhuri Dixit (HR Manager)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (21,1,'2024-09-01','2025-06-30',75000,30000,1600,1250,11500,4000,1500,0,'2024-09-01','2024-09-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (21,2,'2025-07-01',NULL,82000,32800,1600,1250,12500,4500,1500,1,'2025-07-01','2025-07-01',1);

-- EmpId=22: John Abraham (IT SSE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (22,1,'2024-09-01','2025-06-30',58000,23200,1600,1250,8500,3500,1200,0,'2024-09-01','2024-09-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (22,2,'2025-07-01',NULL,64000,25600,1600,1250,9300,3900,1200,1,'2025-07-01','2025-07-01',1);

-- EmpId=23: Parineeti Chopra (Finance Analyst)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (23,1,'2024-09-01','2025-06-30',40000,16000,1600,1250,5500,2200,800,0,'2024-09-01','2024-09-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (23,2,'2025-07-01',NULL,44000,17600,1600,1250,6000,2400,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=24: Tiger Shroff (SWE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (24,1,'2024-09-15','2025-06-30',36000,14400,1600,1250,4800,2000,800,0,'2024-09-15','2024-09-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (24,2,'2025-07-01',NULL,40000,16000,1600,1250,5300,2200,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=25: Jacqueline Fernandez (Marketing Exec)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (25,1,'2024-09-15','2025-06-30',28000,11200,1600,1250,3800,1600,600,0,'2024-09-15','2024-09-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (25,2,'2025-07-01',NULL,31000,12400,1600,1250,4200,1800,600,1,'2025-07-01','2025-07-01',1);

-- EmpId=26: Vikram Chandra (Payroll Analyst)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (26,1,'2024-10-01','2025-06-30',48000,19200,1600,1250,7000,3000,1000,0,'2024-10-01','2024-10-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (26,2,'2025-07-01',NULL,53000,21200,1600,1250,7700,3300,1000,1,'2025-07-01','2025-07-01',1);

-- EmpId=27: Taapsee Pannu (SWE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (27,1,'2024-10-01','2025-06-30',36000,14400,1600,1250,4800,2000,800,0,'2024-10-01','2024-10-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (27,2,'2025-07-01',NULL,40000,16000,1600,1250,5300,2200,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=28: Prabhas (IT TL)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (28,1,'2024-10-01','2025-06-30',70000,28000,1600,1250,10500,4000,1500,0,'2024-10-01','2024-10-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (28,2,'2025-07-01',NULL,77000,30800,1600,1250,11500,4400,1500,1,'2025-07-01','2025-07-01',1);

-- EmpId=29: Samantha Ruth (Marketing Specialist)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (29,1,'2024-10-15','2025-06-30',35000,14000,1600,1250,4600,1900,750,0,'2024-10-15','2024-10-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (29,2,'2025-07-01',NULL,38500,15400,1600,1250,5100,2100,750,1,'2025-07-01','2025-07-01',1);

-- EmpId=30: Allu Arjun (Sales Executive)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (30,1,'2024-10-15','2025-06-30',30000,12000,1600,1250,4000,1700,650,0,'2024-10-15','2024-10-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (30,2,'2025-07-01',NULL,33000,13200,1600,1250,4400,1900,650,1,'2025-07-01','2025-07-01',1);

-- EmpId=31: Rashmika Mandanna (Ops Analyst)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (31,1,'2024-11-01','2025-06-30',36000,14400,1600,1250,4800,2000,750,0,'2024-11-01','2024-11-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (31,2,'2025-07-01',NULL,40000,16000,1600,1250,5300,2200,750,1,'2025-07-01','2025-07-01',1);

-- EmpId=32: NTR Jr (IT SSE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (32,1,'2024-11-01','2025-06-30',55000,22000,1600,1250,8000,3200,1100,0,'2024-11-01','2024-11-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (32,2,'2025-07-01',NULL,60500,24200,1600,1250,8800,3500,1100,1,'2025-07-01','2025-07-01',1);

-- EmpId=33: Vijay Deverakonda (Sales Manager)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (33,1,'2024-11-01','2025-06-30',70000,28000,1600,1250,10500,4000,1500,0,'2024-11-01','2024-11-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (33,2,'2025-07-01',NULL,77000,30800,1600,1250,11500,4400,1500,1,'2025-07-01','2025-07-01',1);

-- EmpId=34: Nayanthara (HR BP)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (34,1,'2024-11-15','2025-06-30',50000,20000,1600,1250,7500,3000,1100,0,'2024-11-15','2024-11-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (34,2,'2025-07-01',NULL,55000,22000,1600,1250,8200,3300,1100,1,'2025-07-01','2025-07-01',1);

-- EmpId=35: Dhanush (IT SWE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (35,1,'2024-11-15','2025-06-30',38000,15200,1600,1250,5000,2100,800,0,'2024-11-15','2024-11-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (35,2,'2025-07-01',NULL,42000,16800,1600,1250,5500,2300,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=36: Kajal Aggarwal (Finance Manager)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (36,1,'2024-12-01','2025-06-30',78000,31200,1600,1250,12000,4500,1700,0,'2024-12-01','2024-12-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (36,2,'2025-07-01',NULL,86000,34400,1600,1250,13000,5000,1700,1,'2025-07-01','2025-07-01',1);

-- EmpId=37: Suriya (Operations Manager)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (37,1,'2024-12-01','2025-06-30',76000,30400,1600,1250,11500,4500,1700,0,'2024-12-01','2024-12-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (37,2,'2025-07-01',NULL,84000,33600,1600,1250,12500,5000,1700,1,'2025-07-01','2025-07-01',1);

-- EmpId=38: Pooja Hegde (HR Executive)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (38,1,'2024-12-15','2025-06-30',30000,12000,1600,1250,4000,1800,700,0,'2024-12-15','2024-12-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (38,2,'2025-07-01',NULL,33000,13200,1600,1250,4400,2000,700,1,'2025-07-01','2025-07-01',1);

-- EmpId=39: Ram Charan (IT SSE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (39,1,'2024-12-15','2025-06-30',60000,24000,1600,1250,9000,3500,1200,0,'2024-12-15','2024-12-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (39,2,'2025-07-01',NULL,66000,26400,1600,1250,9900,3900,1200,1,'2025-07-01','2025-07-01',1);

-- EmpId=40: Kartik Aaryan (Marketing Specialist)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (40,1,'2025-01-02','2025-06-30',35000,14000,1600,1250,4600,1900,750,0,'2025-01-02','2025-01-02',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (40,2,'2025-07-01',NULL,38500,15400,1600,1250,5100,2100,750,1,'2025-07-01','2025-07-01',1);

-- EmpId=41: Kriti Sanon (Legal Associate)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (41,1,'2025-01-02','2025-06-30',45000,18000,1600,1250,6500,2800,1000,0,'2025-01-02','2025-01-02',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (41,2,'2025-07-01',NULL,50000,20000,1600,1250,7200,3100,1000,1,'2025-07-01','2025-07-01',1);

-- EmpId=42: Ayushmann Khurrana (SWE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (42,1,'2025-01-15','2025-06-30',36000,14400,1600,1250,4800,2000,800,0,'2025-01-15','2025-01-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (42,2,'2025-07-01',NULL,40000,16000,1600,1250,5300,2200,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=43: Bhumi Pednekar (Finance Analyst)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (43,1,'2025-01-15','2025-06-30',40000,16000,1600,1250,5500,2200,800,0,'2025-01-15','2025-01-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (43,2,'2025-07-01',NULL,44000,17600,1600,1250,6000,2400,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=44: Rajkumar Rao (IT SSE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (44,1,'2025-02-01','2025-06-30',58000,23200,1600,1250,8500,3500,1100,0,'2025-02-01','2025-02-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (44,2,'2025-07-01',NULL,64000,25600,1600,1250,9300,3900,1100,1,'2025-07-01','2025-07-01',1);

-- EmpId=45: Janhvi Kapoor (Ops Analyst)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (45,1,'2025-02-01','2025-06-30',36000,14400,1600,1250,4800,2000,750,0,'2025-02-01','2025-02-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (45,2,'2025-07-01',NULL,40000,16000,1600,1250,5300,2200,750,1,'2025-07-01','2025-07-01',1);

-- EmpId=46: Vicky Kaushal (SWE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (46,1,'2025-02-15','2025-06-30',38000,15200,1600,1250,5000,2100,800,0,'2025-02-15','2025-02-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (46,2,'2025-07-01',NULL,42000,16800,1600,1250,5500,2300,800,1,'2025-07-01','2025-07-01',1);

-- EmpId=47: Sara Ali Khan (Marketing Exec)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (47,1,'2025-02-15','2025-06-30',28000,11200,1600,1250,3800,1600,600,0,'2025-02-15','2025-02-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (47,2,'2025-07-01',NULL,31000,12400,1600,1250,4200,1800,600,1,'2025-07-01','2025-07-01',1);

-- EmpId=48: Aditya Roy (Payroll Processor)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (48,1,'2025-03-01','2025-06-30',42000,16800,1600,1250,6000,2500,900,0,'2025-03-01','2025-03-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (48,2,'2025-07-01',NULL,46500,18600,1600,1250,6600,2700,900,1,'2025-07-01','2025-07-01',1);

-- EmpId=49: Kiara Advani (Sales Exec)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (49,1,'2025-03-01','2025-06-30',30000,12000,1600,1250,4000,1700,650,0,'2025-03-01','2025-03-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (49,2,'2025-07-01',NULL,33000,13200,1600,1250,4400,1900,650,1,'2025-07-01','2025-07-01',1);

-- EmpId=50: Sonu Sood (General Counsel)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (50,1,'2025-03-15','2025-06-30',85000,34000,1600,1250,14000,5000,2000,0,'2025-03-15','2025-03-15',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (50,2,'2025-07-01',NULL,93000,37200,1600,1250,15500,5500,2000,1,'2025-07-01','2025-07-01',1);

-- EmpId=51: Tamannaah Bhatia (SWE)
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (51,1,'2025-04-01','2025-06-30',38000,15200,1600,1250,5000,2100,800,0,'2025-04-01','2025-04-01',1);
INSERT INTO SalaryStructures (EmployeeId,PolicyId,EffectiveFrom,EffectiveTo,BasicSalary,HRA,ConveyanceAllowance,MedicalAllowance,SpecialAllowance,LTA,OtherAllowances,IsActive,CreatedAt,UpdatedAt,CreatedBy)
VALUES (51,2,'2025-07-01',NULL,42000,16800,1600,1250,5500,2300,800,1,'2025-07-01','2025-07-01',1);

GO

-- =============================================================
-- STEP 11: EMPLOYEE BENEFITS ENROLLMENT
-- All employees get Health Insurance (BenefitId=1), Term Life (2), Transport (4), Meal (5)
-- Senior employees (Mgrs, TLs) get ESOP (6) and PBP (7)
-- =============================================================

-- Batch enroll all 51 employees in standard benefits (HI, TLI, TA, MC)
DECLARE @empId INT = 1;
WHILE @empId <= 51
BEGIN
    INSERT INTO EmployeeBenefits (EmployeeId,BenefitId,EffectiveFrom,IsActive,CreatedAt,UpdatedAt,CreatedBy)
    VALUES
    (@empId, 1, (SELECT CAST(JoiningDate AS DATE) FROM Employees WHERE EmployeeId=@empId), 1, GETUTCDATE(), GETUTCDATE(), 1),
    (@empId, 2, (SELECT CAST(JoiningDate AS DATE) FROM Employees WHERE EmployeeId=@empId), 1, GETUTCDATE(), GETUTCDATE(), 1),
    (@empId, 4, (SELECT CAST(JoiningDate AS DATE) FROM Employees WHERE EmployeeId=@empId), 1, GETUTCDATE(), GETUTCDATE(), 1),
    (@empId, 5, (SELECT CAST(JoiningDate AS DATE) FROM Employees WHERE EmployeeId=@empId), 1, GETUTCDATE(), GETUTCDATE(), 1);
    SET @empId = @empId + 1;
END

-- ESOP and PBP for senior employees (EmpId 1-6, 12, 16, 21, 22, 28, 33, 36, 37, 50)
DECLARE @seniorIds TABLE (EmpId INT);
INSERT INTO @seniorIds VALUES (1),(2),(3),(4),(5),(6),(12),(16),(21),(22),(28),(33),(36),(37),(50);
INSERT INTO EmployeeBenefits (EmployeeId,BenefitId,EffectiveFrom,IsActive,CreatedAt,UpdatedAt,CreatedBy)
SELECT EmpId, 6, (SELECT CAST(JoiningDate AS DATE) FROM Employees WHERE EmployeeId=s.EmpId), 1, GETUTCDATE(), GETUTCDATE(), 1
FROM @seniorIds s;
INSERT INTO EmployeeBenefits (EmployeeId,BenefitId,EffectiveFrom,IsActive,CreatedAt,UpdatedAt,CreatedBy)
SELECT EmpId, 7, (SELECT CAST(JoiningDate AS DATE) FROM Employees WHERE EmployeeId=s.EmpId), 1, GETUTCDATE(), GETUTCDATE(), 1
FROM @seniorIds s;
GO

-- =============================================================
-- STEP 12: PAYROLLS - 2 years monthly payroll for all employees
-- We'll generate payroll from each employee's joining month onward
-- through June 2026. Using a stored calculation approach.
-- PolicyId=1 for Jul24-Jun25, PolicyId=2 for Jul25-Jun26
-- SalaryStructureId: employees have 2 salary structs each (policy1 and policy2)
-- =============================================================

-- We'll use a helper to calculate payroll fields
-- PF = 12% of basic (max ₹1800), ESI = 0.75% of gross if gross<=21000
-- Prof Tax = 200, Income Tax varies by level

-- Insert payrolls month by month for each employee
-- For brevity, we generate all months using a loop approach

DECLARE @MonthStart DATE;
DECLARE @MonthEnd DATE;
DECLARE @PayDate DATE;
DECLARE @EmpId INT;
DECLARE @PolicyId INT;
DECLARE @SalStructId INT;
DECLARE @Basic DECIMAL(18,2), @HRA DECIMAL(18,2), @Conv DECIMAL(18,2), @Med DECIMAL(18,2), @Spl DECIMAL(18,2), @OtherAllow DECIMAL(18,2);
DECLARE @Gross DECIMAL(18,2), @PfEmp DECIMAL(18,2), @PfEr DECIMAL(18,2), @EsiEmp DECIMAL(18,2), @EsiEr DECIMAL(18,2), @PT DECIMAL(18,2), @IT DECIMAL(18,2), @TotalDed DECIMAL(18,2), @Net DECIMAL(18,2);
DECLARE @PresentDays INT, @LeaveDays INT, @WorkingDays INT;
DECLARE @JoinDate DATE;

-- Month loop
DECLARE @LoopMonth DATE = '2024-07-01';
DECLARE @EndMonth DATE = '2026-06-01';

WHILE @LoopMonth <= @EndMonth
BEGIN
    SET @MonthStart = @LoopMonth;
    SET @MonthEnd = EOMONTH(@LoopMonth);
    SET @PayDate = DATEADD(DAY, 5, @MonthEnd); -- paid 5 days after month end

    -- Determine policy
    IF @LoopMonth < '2025-07-01'
        SET @PolicyId = 1
    ELSE
        SET @PolicyId = 2;

    -- Employee loop
    SET @EmpId = 1;
    WHILE @EmpId <= 51
    BEGIN
        -- Get joining date
        SELECT @JoinDate = CAST(JoiningDate AS DATE) FROM Employees WHERE EmployeeId = @EmpId;

        -- Only process payroll if employee has joined by this month
        IF @JoinDate <= @MonthEnd
        BEGIN
            -- Get correct salary structure
            SELECT TOP 1 @SalStructId = SalaryStructureId,
                   @Basic = BasicSalary, @HRA = HRA, @Conv = ConveyanceAllowance,
                   @Med = MedicalAllowance, @Spl = SpecialAllowance, @OtherAllow = OtherAllowances
            FROM SalaryStructures
            WHERE EmployeeId = @EmpId AND PolicyId = @PolicyId;

            IF @SalStructId IS NOT NULL
            BEGIN
                -- Simulate attendance (mostly full, occasionally 1-2 leaves)
                SET @WorkingDays = 26;
                SET @LeaveDays = CASE
                    WHEN @EmpId % 7 = 0 AND MONTH(@LoopMonth) IN (3, 9) THEN 2
                    WHEN @EmpId % 5 = 0 AND MONTH(@LoopMonth) IN (6, 12) THEN 1
                    WHEN @EmpId % 11 = 0 AND MONTH(@LoopMonth) = 8 THEN 3
                    ELSE 0
                END;
                SET @PresentDays = @WorkingDays - @LeaveDays;

                -- Scale salary for partial month (first month)
                DECLARE @ScaleFactor DECIMAL(5,4) = 1.0;
                IF MONTH(@JoinDate) = MONTH(@LoopMonth) AND YEAR(@JoinDate) = YEAR(@LoopMonth)
                    SET @ScaleFactor = CAST((@WorkingDays - DAY(@JoinDate) + 1) AS DECIMAL) / @WorkingDays;

                DECLARE @ScaledBasic DECIMAL(18,2) = ROUND(@Basic * @ScaleFactor, 2);
                DECLARE @ScaledHRA DECIMAL(18,2) = ROUND(@HRA * @ScaleFactor, 2);

                -- Gross = full components (standard, not scaled except basic/HRA for joins)
                SET @Gross = @ScaledBasic + @ScaledHRA + @Conv + @Med + @Spl + @OtherAllow;

                -- Deductions
                SET @PfEmp = ROUND(@ScaledBasic * 0.12, 2);
                IF @PfEmp > 1800 SET @PfEmp = 1800;
                SET @PfEr = @PfEmp; -- employer also 12%
                SET @EsiEmp = CASE WHEN @Gross <= 21000 THEN ROUND(@Gross * 0.0075, 2) ELSE 0 END;
                SET @EsiEr  = CASE WHEN @Gross <= 21000 THEN ROUND(@Gross * 0.0325, 2) ELSE 0 END;
                SET @PT = 200;
                -- Simple income tax: 10% of gross if >50000, 20% if >100000, else 5%
                SET @IT = CASE
                    WHEN @Gross > 100000 THEN ROUND(@Gross * 0.20, 2)
                    WHEN @Gross > 50000  THEN ROUND(@Gross * 0.10, 2)
                    WHEN @Gross > 25000  THEN ROUND(@Gross * 0.05, 2)
                    ELSE 0
                END;
                SET @TotalDed = @PfEmp + @EsiEmp + @PT + @IT;
                SET @Net = @Gross - @TotalDed;

                -- Add bonus in March (annual bonus = 1 month basic)
                DECLARE @Bonus DECIMAL(18,2) = 0;
                IF MONTH(@LoopMonth) = 3 SET @Bonus = @Basic;

                SET @Gross = @Gross + @Bonus;
                SET @Net = @Net + @Bonus;

                -- Status: older months are Paid, last 2 months are Approved
                DECLARE @PayStatus NVARCHAR(20);
                IF @LoopMonth < DATEADD(MONTH, -1, GETDATE())
                    SET @PayStatus = 'Paid'
                ELSE IF @LoopMonth < GETDATE()
                    SET @PayStatus = 'Approved'
                ELSE
                    SET @PayStatus = 'Draft';

                INSERT INTO Payrolls (
                    EmployeeId, PolicyId, SalaryStructureId,
                    PayPeriodStart, PayPeriodEnd, PaymentDate,
                    WorkingDays, PresentDays, LeaveDays, OvertimeHours,
                    BasicSalary, HRA, ConveyanceAllow, MedicalAllow, SpecialAllow, OtherAllowances,
                    OvertimePay, BonusAmount, GrossEarnings,
                    PfEmployee, PfEmployer, EsiEmployee, EsiEmployer, ProfessionalTax, IncomeTax,
                    OtherDeductions, TotalDeductions, NetSalary,
                    Status, ProcessedById, ProcessedAt, ApprovedById, ApprovedAt,
                    Remarks, CreatedAt, UpdatedAt, CreatedBy
                )
                VALUES (
                    @EmpId, @PolicyId, @SalStructId,
                    @MonthStart, @MonthEnd, CASE WHEN @PayStatus IN ('Paid','Approved') THEN @PayDate ELSE NULL END,
                    @WorkingDays, @PresentDays, @LeaveDays, 0,
                    @ScaledBasic, @ScaledHRA, @Conv, @Med, @Spl, @OtherAllow,
                    0, @Bonus, @Gross,
                    @PfEmp, @PfEr, @EsiEmp, @EsiEr, @PT, @IT,
                    0, @TotalDed, @Net,
                    @PayStatus,
                    CASE WHEN @PayStatus IN ('Paid','Approved') THEN 2 ELSE NULL END,
                    CASE WHEN @PayStatus IN ('Paid','Approved') THEN CAST(@MonthEnd AS DATETIME2) ELSE NULL END,
                    CASE WHEN @PayStatus = 'Paid' THEN 1 ELSE NULL END,
                    CASE WHEN @PayStatus = 'Paid' THEN CAST(@PayDate AS DATETIME2) ELSE NULL END,
                    'Monthly payroll', GETUTCDATE(), GETUTCDATE(), 1
                );
            END
        END

        SET @EmpId = @EmpId + 1;
    END

    SET @LoopMonth = DATEADD(MONTH, 1, @LoopMonth);
END
GO

-- =============================================================
-- STEP 13: LEAVE REQUESTS - realistic leave history
-- =============================================================

-- Batch insert leave requests for employees
-- Each employee gets various leaves across the 2 years
INSERT INTO LeaveRequests (EmployeeId,LeaveTypeId,FromDate,ToDate,TotalDays,Reason,Status,IsHalfDay,ApprovedById,ApprovedAt,CreatedAt,UpdatedAt)
VALUES
-- EmpId=7 Sugan (casual, sick, privilege)
(7,  1, '2024-08-12', '2024-08-13', 2, 'Personal work',           'Approved', 0, 3,  '2024-08-10', '2024-08-08', '2024-08-10'),
(7,  2, '2024-10-05', '2024-10-05', 1, 'Fever and cold',          'Approved', 0, 3,  '2024-10-05', '2024-10-05', '2024-10-05'),
(7,  3, '2024-12-23', '2024-12-27', 5, 'Year-end vacation',       'Approved', 0, 3,  '2024-12-20', '2024-12-18', '2024-12-20'),
(7,  1, '2025-02-14', '2025-02-14', 1, 'Personal occasion',       'Approved', 0, 3,  '2025-02-13', '2025-02-12', '2025-02-13'),
(7,  2, '2025-05-20', '2025-05-21', 2, 'Medical checkup',         'Approved', 0, 3,  '2025-05-19', '2025-05-18', '2025-05-19'),
(7,  3, '2025-12-22', '2025-12-31', 5, 'Christmas holidays',      'Approved', 0, 3,  '2025-12-19', '2025-12-18', '2025-12-19'),
-- EmpId=8 Shah Rukh
(8,  1, '2024-09-02', '2024-09-03', 2, 'Personal work',           'Approved', 0, 3,  '2024-08-30', '2024-08-28', '2024-08-30'),
(8,  2, '2024-11-14', '2024-11-15', 2, 'Stomach infection',       'Approved', 0, 3,  '2024-11-13', '2024-11-13', '2024-11-13'),
(8,  3, '2025-01-13', '2025-01-17', 5, 'Pongal vacation trip',    'Approved', 0, 3,  '2025-01-10', '2025-01-08', '2025-01-10'),
(8,  1, '2025-06-16', '2025-06-16', 1, 'Personal occasion',       'Approved', 0, 3,  '2025-06-14', '2025-06-13', '2025-06-14'),
-- EmpId=1 Amitabh (HR Manager - takes privilege)
(1,  3, '2024-10-11', '2024-10-15', 5, 'Birthday vacation',       'Approved', 0, 21, '2024-10-08', '2024-10-07', '2024-10-08'),
(1,  1, '2025-03-03', '2025-03-04', 2, 'Personal work Mumbai',    'Approved', 0, 21, '2025-03-01', '2025-02-28', '2025-03-01'),
(1,  3, '2025-11-10', '2025-11-14', 5, 'Annual leave',            'Approved', 0, 21, '2025-11-07', '2025-11-06', '2025-11-07'),
-- EmpId=3 Aamir Khan (Manager)
(3,  3, '2024-09-09', '2024-09-13', 5, 'Family vacation Goa',     'Approved', 0, 1,  '2024-09-06', '2024-09-04', '2024-09-06'),
(3,  1, '2025-04-14', '2025-04-14', 1, 'Good Friday',             'Approved', 0, 1,  '2025-04-12', '2025-04-11', '2025-04-12'),
-- EmpId=4 Deepika (Manager)
(4,  3, '2024-12-23', '2024-12-31', 7, 'Year-end trip abroad',    'Approved', 0, 1,  '2024-12-20', '2024-12-18', '2024-12-20'),
(4,  1, '2025-08-15', '2025-08-15', 1, 'Independence Day holiday','Approved', 0, 1,  '2025-08-14', '2025-08-13', '2025-08-14'),
-- EmpId=9 Katrina (sick + casual)
(9,  2, '2024-10-21', '2024-10-22', 2, 'Viral fever',             'Approved', 0, 4,  '2024-10-21', '2024-10-21', '2024-10-21'),
(9,  1, '2025-01-20', '2025-01-20', 1, 'Personal work',           'Approved', 0, 4,  '2025-01-18', '2025-01-17', '2025-01-18'),
(9,  3, '2025-07-07', '2025-07-11', 5, 'Summer vacation',         'Approved', 0, 4,  '2025-07-04', '2025-07-03', '2025-07-04'),
-- EmpId=10 Salman (sick)
(10, 2, '2024-09-18', '2024-09-18', 1, 'Back pain',               'Approved', 0, 5,  '2024-09-18', '2024-09-18', '2024-09-18'),
(10, 1, '2025-02-03', '2025-02-04', 2, 'Family function',         'Approved', 0, 5,  '2025-02-01', '2025-01-30', '2025-02-01'),
-- EmpId=11 Kareena (maternity leave)
(11, 4, '2025-03-01', '2025-05-29', 90,'Maternity leave',         'Approved', 0, 1,  '2025-02-25', '2025-02-20', '2025-02-25'),
-- EmpId=12 Ranbir
(12, 1, '2024-11-04', '2024-11-05', 2, 'Personal work',           'Approved', 0, 3,  '2024-11-02', '2024-11-01', '2024-11-02'),
(12, 3, '2025-06-02', '2025-06-06', 5, 'Annual vacation trip',    'Approved', 0, 3,  '2025-05-30', '2025-05-28', '2025-05-30'),
-- EmpId=13 Alia
(13, 2, '2024-08-19', '2024-08-20', 2, 'Migraine episode',        'Approved', 0, 1,  '2024-08-19', '2024-08-19', '2024-08-19'),
(13, 3, '2025-04-21', '2025-04-25', 5, 'Vacation to Shimla',      'Approved', 0, 1,  '2025-04-18', '2025-04-17', '2025-04-18'),
-- EmpId=14 Varun
(14, 1, '2024-10-14', '2024-10-15', 2, 'Family engagement',       'Approved', 0, 3,  '2024-10-12', '2024-10-11', '2024-10-12'),
(14, 2, '2025-01-06', '2025-01-07', 2, 'Throat infection',        'Approved', 0, 3,  '2025-01-06', '2025-01-06', '2025-01-06'),
-- EmpId=15 Anushka
(15, 5, '2024-11-25', '2024-11-26', 2, 'Comp off for Diwali work','Approved', 0, 1,  '2024-11-24', '2024-11-22', '2024-11-24'),
-- EmpId=16 Hrithik (privilege + casual)
(16, 3, '2024-12-02', '2024-12-06', 5, 'Team offsite trip',       'Approved', 0, 3,  '2024-11-29', '2024-11-27', '2024-11-29'),
(16, 1, '2025-03-24', '2025-03-24', 1, 'Personal work',           'Approved', 0, 3,  '2025-03-22', '2025-03-21', '2025-03-22'),
-- EmpId=17 Sonam
(17, 2, '2024-09-09', '2024-09-10', 2, 'Fever and body ache',     'Approved', 0, 1,  '2024-09-09', '2024-09-09', '2024-09-09'),
-- EmpId=18 Akshay
(18, 3, '2025-01-27', '2025-01-31', 5, 'Republic Day + vacation', 'Approved', 0, 6,  '2025-01-24', '2025-01-23', '2025-01-24'),
-- EmpId=19 Shraddha
(19, 1, '2024-09-30', '2024-09-30', 1, 'Personal work',           'Approved', 0, 4,  '2024-09-28', '2024-09-27', '2024-09-28'),
(19, 2, '2025-02-10', '2025-02-11', 2, 'Seasonal flu',            'Approved', 0, 4,  '2025-02-10', '2025-02-10', '2025-02-10'),
-- EmpId=20 Siddharth
(20, 1, '2025-04-07', '2025-04-08', 2, 'Family function',         'Approved', 0, 5,  '2025-04-05', '2025-04-04', '2025-04-05'),
-- EmpId=21 Madhuri
(21, 3, '2025-01-06', '2025-01-10', 5, 'New Year vacation',       'Approved', 0, 1,  '2025-01-03', '2025-01-02', '2025-01-03'),
-- EmpId=22 John
(22, 2, '2024-10-28', '2024-10-29', 2, 'Eye infection',           'Approved', 0, 3,  '2024-10-28', '2024-10-28', '2024-10-28'),
-- EmpId=23 Parineeti
(23, 1, '2025-02-17', '2025-02-18', 2, 'Personal occasion',       'Approved', 0, 1,  '2025-02-15', '2025-02-14', '2025-02-15'),
-- EmpId=24 Tiger
(24, 2, '2024-12-09', '2024-12-10', 2, 'Fitness injury rest',     'Approved', 0, 3,  '2024-12-09', '2024-12-09', '2024-12-09'),
-- EmpId=25 Jacqueline
(25, 1, '2025-05-05', '2025-05-05', 1, 'Personal work',           'Approved', 0, 4,  '2025-05-03', '2025-05-02', '2025-05-03'),
-- EmpId=26 Vikram
(26, 3, '2025-04-14', '2025-04-18', 5, 'Vacation to Ooty',        'Approved', 0, 2,  '2025-04-11', '2025-04-10', '2025-04-11'),
-- EmpId=27 Taapsee
(27, 2, '2025-01-20', '2025-01-21', 2, 'Cold and cough',          'Approved', 0, 3,  '2025-01-20', '2025-01-20', '2025-01-20'),
-- EmpId=28 Prabhas
(28, 3, '2025-03-17', '2025-03-21', 5, 'Ugadi vacation',          'Approved', 0, 3,  '2025-03-14', '2025-03-13', '2025-03-14'),
-- EmpId=29 Samantha
(29, 4, '2025-06-09', '2025-09-06', 90,'Maternity leave',         'Approved', 0, 1,  '2025-06-05', '2025-06-04', '2025-06-05'),
-- EmpId=30 Allu Arjun
(30, 5, '2025-01-13', '2025-01-13', 1, 'Comp off for extra work', 'Approved', 0, 5,  '2025-01-12', '2025-01-11', '2025-01-12'),
-- EmpId=31 Rashmika
(31, 1, '2024-12-16', '2024-12-17', 2, 'Personal work Bengaluru', 'Approved', 0, 6,  '2024-12-14', '2024-12-13', '2024-12-14'),
-- EmpId=32 NTR Jr
(32, 2, '2025-01-27', '2025-01-28', 2, 'Viral infection',         'Approved', 0, 3,  '2025-01-27', '2025-01-27', '2025-01-27'),
-- EmpId=33 Vijay D.
(33, 3, '2025-08-18', '2025-08-22', 5, 'Annual leave',            'Approved', 0, 5,  '2025-08-15', '2025-08-14', '2025-08-15'),
-- EmpId=34 Nayanthara
(34, 1, '2025-02-24', '2025-02-25', 2, 'Personal occasion',       'Approved', 0, 1,  '2025-02-22', '2025-02-21', '2025-02-22'),
-- EmpId=35 Dhanush
(35, 2, '2025-03-03', '2025-03-04', 2, 'Severe headache',         'Approved', 0, 3,  '2025-03-03', '2025-03-03', '2025-03-03'),
-- EmpId=36 Kajal (Finance Manager)
(36, 3, '2025-05-19', '2025-05-23', 5, 'Annual leave',            'Approved', 0, 1,  '2025-05-16', '2025-05-15', '2025-05-16'),
-- EmpId=37 Suriya
(37, 5, '2025-01-26', '2025-01-27', 2, 'Comp off for overtime',   'Approved', 0, 6,  '2025-01-25', '2025-01-24', '2025-01-25'),
-- EmpId=38 Pooja
(38, 1, '2025-03-21', '2025-03-21', 1, 'Personal work',           'Approved', 0, 1,  '2025-03-19', '2025-03-18', '2025-03-19'),
-- EmpId=39 Ram Charan
(39, 3, '2025-04-28', '2025-05-02', 5, 'Summer vacation',         'Approved', 0, 3,  '2025-04-25', '2025-04-24', '2025-04-25'),
-- EmpId=40 Kartik
(40, 2, '2025-03-10', '2025-03-11', 2, 'Cold and fever',          'Approved', 0, 4,  '2025-03-10', '2025-03-10', '2025-03-10'),
-- EmpId=41 Kriti
(41, 1, '2025-03-24', '2025-03-25', 2, 'Personal occasion',       'Approved', 0, 1,  '2025-03-22', '2025-03-21', '2025-03-22'),
-- EmpId=42 Ayushmann
(42, 2, '2025-04-07', '2025-04-08', 2, 'Allergy and breathing',   'Approved', 0, 3,  '2025-04-07', '2025-04-07', '2025-04-07'),
-- EmpId=43 Bhumi
(43, 1, '2025-04-21', '2025-04-22', 2, 'Family visit',            'Approved', 0, 1,  '2025-04-19', '2025-04-18', '2025-04-19'),
-- EmpId=44 Rajkumar
(44, 3, '2025-05-26', '2025-05-30', 5, 'Vacation to Manali',      'Approved', 0, 3,  '2025-05-23', '2025-05-22', '2025-05-23'),
-- EmpId=45 Janhvi
(45, 2, '2025-04-14', '2025-04-15', 2, 'Dengue fever rest',       'Approved', 0, 6,  '2025-04-14', '2025-04-14', '2025-04-14'),
-- EmpId=46 Vicky
(46, 5, '2025-04-28', '2025-04-28', 1, 'Comp off',                'Approved', 0, 3,  '2025-04-26', '2025-04-25', '2025-04-26'),
-- EmpId=47 Sara Ali
(47, 1, '2025-05-12', '2025-05-13', 2, 'Family function',         'Approved', 0, 4,  '2025-05-10', '2025-05-09', '2025-05-10'),
-- EmpId=48 Aditya
(48, 2, '2025-05-19', '2025-05-20', 2, 'Fever',                   'Approved', 0, 2,  '2025-05-19', '2025-05-19', '2025-05-19'),
-- EmpId=49 Kiara
(49, 3, '2025-05-26', '2025-05-30', 5, 'Goa vacation',            'Approved', 0, 5,  '2025-05-23', '2025-05-22', '2025-05-23'),
-- EmpId=50 Sonu
(50, 1, '2025-06-09', '2025-06-10', 2, 'Personal work',           'Approved', 0, 1,  '2025-06-07', '2025-06-06', '2025-06-07'),
-- EmpId=51 Tamannaah
(51, 2, '2025-06-16', '2025-06-17', 2, 'Seasonal allergies',      'Approved', 0, 3,  '2025-06-16', '2025-06-16', '2025-06-16'),
-- Some pending leaves for current month (July 2026)
(7,  1, '2026-07-14', '2026-07-15', 2, 'Personal work',           'Pending',  0, NULL, NULL,         '2026-07-09', '2026-07-09'),
(8,  3, '2026-07-21', '2026-07-25', 5, 'Annual summer vacation',  'Pending',  0, NULL, NULL,         '2026-07-09', '2026-07-09'),
(12, 2, '2026-07-10', '2026-07-10', 1, 'Not feeling well',        'Pending',  0, NULL, NULL,         '2026-07-09', '2026-07-09'),
(19, 1, '2026-07-17', '2026-07-17', 1, 'Bank work',               'Pending',  0, NULL, NULL,         '2026-07-09', '2026-07-09');

GO

-- =============================================================
-- STEP 14: NOTIFICATIONS
-- =============================================================
INSERT INTO Notifications (UserId, Title, Message, NotificationType, ReferenceType, ReferenceId, IsRead, CreatedAt) VALUES
(2, 'Welcome to EasyPay', 'Welcome Amitabh! Your HR Manager account is ready.', 'Info', NULL, NULL, 1, '2024-07-01'),
(3, 'Welcome to EasyPay', 'Welcome Rajinikanth! Your Payroll Processor account is ready.', 'Info', NULL, NULL, 1, '2024-07-01'),
(8, 'Welcome to EasyPay', 'Welcome Sugan! Your employee account has been created. Your Employee Code is EMP0007.', 'Success', NULL, NULL, 0, '2024-07-15'),
(2, 'New Employee Joined', 'Sugan Prabha has joined the IT Department as Software Engineer on 15-Jul-2024.', 'Info', 'Employee', 7, 1, '2024-07-15'),
(2, 'Payroll Processed - July 2024', 'Monthly payroll for July 2024 has been processed successfully for 6 employees.', 'Success', 'Payroll', NULL, 1, '2024-08-05'),
(3, 'Payroll Ready for Processing', 'August 2024 payroll is ready for processing. Please review and process.', 'Info', 'Payroll', NULL, 1, '2024-08-25'),
(8, 'Payslip Generated', 'Your payslip for July 2024 is now available. Net salary: Check your payslip.', 'Success', 'Payroll', NULL, 1, '2024-08-05'),
(2, 'Leave Request Approved', 'Sugan Prabha leave request for 12-Aug to 13-Aug 2024 has been approved.', 'Success', 'Leave', NULL, 1, '2024-08-10'),
(8, 'Leave Request Approved', 'Your casual leave for 12-Aug to 13-Aug has been approved by your manager.', 'Success', 'Leave', NULL, 1, '2024-08-10'),
(2, 'Salary Structure Updated', 'FY 2025-26 salary structures have been updated for all employees.', 'Info', 'SalaryStructure', NULL, 0, '2025-07-01'),
(3, 'Payroll Policy Updated', 'New payroll policy Standard Policy FY2025-26 is now active.', 'Info', 'PayrollPolicy', NULL, 0, '2025-07-01'),
(8, 'Salary Increment', 'Congratulations! Your salary has been revised effective 1st July 2025. New CTC updated.', 'Success', NULL, NULL, 0, '2025-07-01'),
(2, 'Maternity Leave - Kareena Kapoor', 'Maternity leave for Kareena Kapoor approved for 90 days from 01-Mar-2025.', 'Info', 'Leave', NULL, 1, '2025-02-25'),
(2, 'Year End Payroll Complete', 'FY 2024-25 payroll has been completed successfully. Annual bonus processed.', 'Success', 'Payroll', NULL, 1, '2025-04-05'),
(8, 'Leave Request Pending', 'Your casual leave request for 14-Jul to 15-Jul 2026 is pending approval.', 'Info', 'Leave', NULL, 0, '2026-07-09'),
(2, 'Pending Leave Approvals', 'You have 4 pending leave requests awaiting your approval.', 'Warning', 'Leave', NULL, 0, '2026-07-09'),
(3, 'July 2026 Payroll Due', 'July 2026 payroll processing is pending. Please initiate processing.', 'Warning', 'Payroll', NULL, 0, '2026-07-09');
GO

-- =============================================================
-- STEP 15: TIMESHEETS (sample 30 days for key employees)
-- =============================================================

-- Generate timesheets for Sugan (EmpId=7) for recent months
DECLARE @TsDate DATE = '2026-06-01';
DECLARE @TsEnd DATE = '2026-07-08'; -- up to yesterday

WHILE @TsDate <= @TsEnd
BEGIN
    -- Skip weekends (Sat=7, Sun=1)
    IF DATEPART(WEEKDAY, @TsDate) NOT IN (1, 7)
    BEGIN
        DECLARE @HoursW DECIMAL(5,2) = CASE
            WHEN DATEPART(WEEKDAY, @TsDate) IN (6) THEN 7.5  -- Friday shorter
            ELSE 8.5
        END;
        DECLARE @OTHours DECIMAL(5,2) = CASE WHEN @HoursW > 8 THEN @HoursW - 8 ELSE 0 END;

        IF NOT EXISTS (SELECT 1 FROM Timesheets WHERE EmployeeId=7 AND WorkDate=@TsDate)
            INSERT INTO Timesheets (EmployeeId,WorkDate,CheckIn,CheckOut,HoursWorked,OvertimeHours,Status,ApprovedById,ApprovedAt,CreatedAt,UpdatedAt)
            VALUES (7, @TsDate, '09:00', CASE WHEN @HoursW = 8.5 THEN '18:30' ELSE '17:30' END, @HoursW, @OTHours,
                    CASE WHEN @TsDate < '2026-07-01' THEN 'Approved' ELSE 'Pending' END,
                    CASE WHEN @TsDate < '2026-07-01' THEN 3 ELSE NULL END,
                    CASE WHEN @TsDate < '2026-07-01' THEN CAST(@TsDate AS DATETIME2) ELSE NULL END,
                    GETUTCDATE(), GETUTCDATE());
    END
    SET @TsDate = DATEADD(DAY, 1, @TsDate);
END

-- Generate timesheets for EmpId=8 (Shah Rukh) same period
SET @TsDate = '2026-06-01';
WHILE @TsDate <= @TsEnd
BEGIN
    IF DATEPART(WEEKDAY, @TsDate) NOT IN (1, 7)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Timesheets WHERE EmployeeId=8 AND WorkDate=@TsDate)
            INSERT INTO Timesheets (EmployeeId,WorkDate,CheckIn,CheckOut,HoursWorked,OvertimeHours,Status,ApprovedById,ApprovedAt,CreatedAt,UpdatedAt)
            VALUES (8, @TsDate, '09:30', '18:30', 8.0, 0.0,
                    CASE WHEN @TsDate < '2026-07-01' THEN 'Approved' ELSE 'Pending' END,
                    CASE WHEN @TsDate < '2026-07-01' THEN 3 ELSE NULL END,
                    CASE WHEN @TsDate < '2026-07-01' THEN CAST(@TsDate AS DATETIME2) ELSE NULL END,
                    GETUTCDATE(), GETUTCDATE());
    END
    SET @TsDate = DATEADD(DAY, 1, @TsDate);
END
GO

PRINT '=============================================================';
PRINT 'EasyPay Seed Data Loaded Successfully!';
PRINT '';
PRINT 'CREDENTIALS SUMMARY:';
PRINT '---------------------';
PRINT 'ALL USERS PASSWORD: EasyPay@123!';
PRINT '';
PRINT '| Role              | Name              | Email                          | Username               |';
PRINT '|-------------------|-------------------|--------------------------------|------------------------|';
PRINT '| Admin             | Admin             | admin@easypay.com              | admin                  |';
PRINT '| HR Manager        | Amitabh Bachchan  | grootthe38@gmail.com           | amitabh.bachchan       |';
PRINT '| Payroll Processor | Rajinikanth       | rajinikanth@easypay.in         | rajinikanth.superstar  |';
PRINT '| Manager (IT)      | Aamir Khan        | aamir.khan@easypay.in          | aamir.khan             |';
PRINT '| Manager (MKT)     | Deepika Padukone  | deepika.padukone@easypay.in    | deepika.padukone       |';
PRINT '| Manager (SAL)     | Ranveer Singh     | ranveer.singh@easypay.in       | ranveer.singh          |';
PRINT '| Manager (OPS)     | Priyanka Chopra   | priyanka.chopra@easypay.in     | priyanka.chopra        |';
PRINT '| Employee          | Sugan Prabha      | suganprabhakaran196@gmail.com  | sugan.prabha           |';
PRINT '| Employee          | Shah Rukh Khan    | shahrukhkhan@easypay.in        | shah.rukh.khan         |';
PRINT '| Employee          | Katrina Kaif      | katrina.kaif@easypay.in        | katrina.kaif           |';
PRINT '| Employee          | Salman Khan       | salman.khan@easypay.in         | salman.khan            |';
PRINT '| Employee          | Kareena Kapoor    | kareena.kapoor@easypay.in      | kareena.kapoor         |';
PRINT '| Employee          | Ranbir Kapoor     | ranbir.kapoor@easypay.in       | ranbir.kapoor          |';
PRINT '| Employee          | Alia Bhatt        | alia.bhatt@easypay.in          | alia.bhatt             |';
PRINT '| Employee          | Varun Dhawan      | varun.dhawan@easypay.in        | varun.dhawan           |';
PRINT '| Employee          | Anushka Sharma    | anushka.sharma@easypay.in      | anushka.sharma         |';
PRINT '| Employee          | Hrithik Roshan    | hrithik.roshan@easypay.in      | hrithik.roshan         |';
PRINT '| Employee          | Sonam Kapoor      | sonam.kapoor@easypay.in        | sonam.kapoor           |';
PRINT '| Employee          | Akshay Kumar      | akshay.kumar@easypay.in        | akshay.kumar           |';
PRINT '| Employee          | Shraddha Kapoor   | shraddha.kapoor@easypay.in     | shraddha.kapoor        |';
PRINT '| Employee          | Siddharth Malhotra| siddharth.malhotra@easypay.in  | siddharth.malhotra     |';
PRINT '| Employee          | Madhuri Dixit     | madhuri.dixit@easypay.in       | madhuri.dixit          |';
PRINT '| Employee          | John Abraham      | john.abraham@easypay.in        | john.abraham           |';
PRINT '| Employee          | Parineeti Chopra  | parineeti.chopra@easypay.in    | parineeti.chopra       |';
PRINT '| Employee          | Tiger Shroff      | tiger.shroff@easypay.in        | tiger.shroff           |';
PRINT '| Employee          | Jacqueline F.     | jacqueline.fernandez@easypay.in| jacqueline.fernandez   |';
PRINT '| Employee          | Vikram Chandra    | vikram.chandra@easypay.in      | vikram.chandra         |';
PRINT '| Employee          | Taapsee Pannu     | taapsee.pannu@easypay.in       | taapsee.pannu          |';
PRINT '| Employee          | Prabhas           | prabhas.rebel@easypay.in       | prabhas.rebel          |';
PRINT '| Employee          | Samantha Ruth     | samantha.ruth@easypay.in       | samantha.ruth          |';
PRINT '| Employee          | Allu Arjun        | allu.arjun@easypay.in          | allu.arjun             |';
PRINT '| Employee          | Rashmika Mandanna | rashmika.mandanna@easypay.in   | rashmika.mandanna      |';
PRINT '| Employee          | NTR Jr            | ntr.jr@easypay.in              | ntr.jr                 |';
PRINT '| Employee          | Vijay Deverakonda | vijay.deverakonda@easypay.in   | vijay.deverakonda      |';
PRINT '| Employee          | Nayanthara        | nayanthara.star@easypay.in     | nayanthara.star        |';
PRINT '| Employee          | Dhanush           | dhanush.kumar@easypay.in       | dhanush.kumar          |';
PRINT '| Employee          | Kajal Aggarwal    | kajal.aggarwal@easypay.in      | kajal.aggarwal         |';
PRINT '| Employee          | Suriya Sivakumar  | suriya.sivakumar@easypay.in    | suriya.sivakumar       |';
PRINT '| Employee          | Pooja Hegde       | pooja.hegde@easypay.in         | pooja.hegde            |';
PRINT '| Employee          | Ram Charan        | ram.charan@easypay.in          | ram.charan             |';
PRINT '| Employee          | Kartik Aaryan     | kartik.aaryan@easypay.in       | kartik.aaryan          |';
PRINT '| Employee          | Kriti Sanon       | kriti.sanon@easypay.in         | kriti.sanon            |';
PRINT '| Employee          | Ayushmann K.      | ayushmann.khurrana@easypay.in  | ayushmann.khurrana     |';
PRINT '| Employee          | Bhumi Pednekar    | bhumi.pednekar@easypay.in      | bhumi.pednekar         |';
PRINT '| Employee          | Rajkumar Rao      | rajkumar.rao@easypay.in        | rajkumar.rao           |';
PRINT '| Employee          | Janhvi Kapoor     | janhvi.kapoor@easypay.in       | janhvi.kapoor          |';
PRINT '| Employee          | Vicky Kaushal     | vicky.kaushal@easypay.in       | vicky.kaushal          |';
PRINT '| Employee          | Sara Ali Khan     | sara.ali.khan@easypay.in       | sara.ali.khan          |';
PRINT '| Employee          | Aditya Roy        | aditya.roy@easypay.in          | aditya.roy             |';
PRINT '| Employee          | Kiara Advani      | kiara.advani@easypay.in        | kiara.advani           |';
PRINT '| Employee          | Sonu Sood         | sonu.sood@easypay.in           | sonu.sood              |';
PRINT '| Employee          | Tamannaah Bhatia  | tamannaah.bhatia@easypay.in    | tamannaah.bhatia       |';
PRINT '=============================================================';
