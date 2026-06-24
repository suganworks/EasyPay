namespace EasyPay.Core.Enums;

public enum UserRole
{
    Admin = 1,
    HRManager = 2,
    PayrollProcessor = 3,
    Employee = 4,
    Manager = 5
}

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Intern
}

public enum EmploymentStatus
{
    Active,
    OnLeave,
    Terminated,
    Resigned
}

public enum Gender
{
    Male,
    Female,
    Other
}

public enum PayFrequency
{
    Weekly,
    BiWeekly,
    Monthly
}

public enum PayrollStatus
{
    Draft,
    Pending,
    Approved,
    Paid,
    Cancelled
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public enum TimesheetStatus
{
    Pending,
    Approved,
    Rejected
}

public enum NotificationType
{
    Info,
    Warning,
    Success,
    Error
}

public enum BenefitType
{
    Health,
    Insurance,
    Retirement,
    Transport,
    Meal,
    Other
}

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Login,
    Logout,
    PasswordChange,
    PasswordReset,
    PayrollProcess,
    PayrollApprove,
    LeaveApprove,
    LeaveReject
}
