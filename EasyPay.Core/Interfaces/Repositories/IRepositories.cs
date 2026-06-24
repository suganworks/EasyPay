using EasyPay.Core.DTOs;
using EasyPay.Core.Entities;

namespace EasyPay.Core.Interfaces.Repositories;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<Employee?> GetByEmployeeCodeAsync(string code);
    Task<Employee?> GetByUserIdAsync(int userId);
    Task<Employee?> GetWithDetailsAsync(int employeeId);
    Task<(IEnumerable<Employee> Items, int Total)> GetPagedAsync(PaginationParams pagination, int? departmentId = null);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetByManagerAsync(int managerId);
    Task<bool> EmployeeCodeExistsAsync(string code);
    Task<string> GenerateEmployeeCodeAsync();
}

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<Department?> GetWithEmployeesAsync(int departmentId);
    Task<bool> DepartmentCodeExistsAsync(string code);
    Task<IEnumerable<Department>> GetActiveAsync();
}

public interface IDesignationRepository : IGenericRepository<Designation>
{
    Task<IEnumerable<Designation>> GetByDepartmentAsync(int departmentId);
    Task<bool> DesignationCodeExistsAsync(string code);
    Task<IEnumerable<Designation>> GetActiveAsync();
}

public interface IPayrollPolicyRepository : IGenericRepository<PayrollPolicy>
{
    Task<PayrollPolicy?> GetActivePolicyAsync();
    Task<IEnumerable<PayrollPolicy>> GetActiveAsync();
}

public interface ISalaryStructureRepository : IGenericRepository<SalaryStructure>
{
    Task<SalaryStructure?> GetCurrentForEmployeeAsync(int employeeId);
    Task<IEnumerable<SalaryStructure>> GetHistoryForEmployeeAsync(int employeeId);
    Task DeactivatePreviousAsync(int employeeId);
}

public interface IPayrollRepository : IGenericRepository<Payroll>
{
    Task<Payroll?> GetWithDetailsAsync(int payrollId);
    Task<(IEnumerable<Payroll> Items, int Total)> GetPagedAsync(PaginationParams pagination,
        int? employeeId = null, string? status = null, int? year = null, int? month = null);
    Task<Payroll?> GetForPeriodAsync(int employeeId, DateOnly periodStart, DateOnly periodEnd);
    Task<IEnumerable<Payroll>> GetByStatusAsync(string status);
    Task<IEnumerable<Payroll>> GetForBulkProcessingAsync(DateOnly periodStart, DateOnly periodEnd);
    Task<bool> PayrollExistsForPeriodAsync(int employeeId, DateOnly periodStart, DateOnly periodEnd);
}

public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
{
    Task<(IEnumerable<LeaveRequest> Items, int Total)> GetPagedAsync(PaginationParams pagination,
        int? employeeId = null, string? status = null);
    Task<IEnumerable<LeaveRequest>> GetPendingForManagerAsync(int managerId);
    Task<int> GetUsedLeaveDaysAsync(int employeeId, int leaveTypeId, int year);
    Task<bool> HasOverlappingLeaveAsync(int employeeId, DateOnly fromDate, DateOnly toDate, int? excludeId = null);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAndYearAsync(int employeeId, int year);
}

public interface ITimesheetRepository : IGenericRepository<Timesheet>
{
    Task<(IEnumerable<Timesheet> Items, int Total)> GetPagedAsync(PaginationParams pagination,
        int? employeeId = null, DateOnly? fromDate = null, DateOnly? toDate = null);
    Task<Timesheet?> GetForDateAsync(int employeeId, DateOnly workDate);
    Task<IEnumerable<Timesheet>> GetForPayrollPeriodAsync(int employeeId, DateOnly start, DateOnly end);
    Task<decimal> GetTotalHoursAsync(int employeeId, DateOnly start, DateOnly end);
    Task<decimal> GetTotalOvertimeAsync(int employeeId, DateOnly start, DateOnly end);
}

public interface IBenefitRepository : IGenericRepository<Benefit>
{
    Task<IEnumerable<Benefit>> GetActiveAsync();
    Task<bool> BenefitCodeExistsAsync(string code);
}

public interface IEmployeeBenefitRepository : IGenericRepository<EmployeeBenefit>
{
    Task<IEnumerable<EmployeeBenefit>> GetActiveForEmployeeAsync(int employeeId);
    Task DeactivatePreviousAsync(int employeeId, int benefitId);
}

public interface ILeaveTypeRepository : IGenericRepository<LeaveType>
{
    Task<IEnumerable<LeaveType>> GetActiveAsync();
    Task<LeaveType?> GetByCodeAsync(string code);
}

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<(IEnumerable<Notification> Items, int Total)> GetPagedForUserAsync(int userId, PaginationParams pagination);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAllAsReadAsync(int userId);
    Task MarkAsReadAsync(int notificationId);
}

public interface IAuditLogRepository
{
    Task LogAsync(AuditLog auditLog);
    Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAsync(PaginationParams pagination,
        int? userId = null, string? action = null, string? entityName = null,
        DateTime? fromDate = null, DateTime? toDate = null);
}
