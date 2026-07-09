using EasyPay.Core.DTOs;
using EasyPay.Core.Entities;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPay.Infrastructure.Repositories;

// ─── EmployeeRepository ───────────────────────────────────────────────────────
public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(EasyPayDbContext context) : base(context) { }

    public async Task<Employee?> GetByEmployeeCodeAsync(string code)
        => await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.EmployeeCode == code);

    public async Task<Employee?> GetByUserIdAsync(int userId)
        => await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.UserId == userId);

    public async Task<Employee?> GetWithDetailsAsync(int employeeId)
        => await _context.Employees
            .Include(e => e.User).ThenInclude(u => u.Role)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

    public async Task<(IEnumerable<Employee> Items, int Total)> GetPagedAsync(
        PaginationParams pagination, int? departmentId = null, int? designationId = null)
    {
        var query = _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (designationId.HasValue)
            query = query.Where(e => e.DesignationId == designationId.Value);

        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var term = pagination.SearchTerm.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term)  ||
                e.EmployeeCode.ToLower().Contains(term) ||
                e.User.Email.ToLower().Contains(term));
        }

        var total = await query.CountAsync();

        query = pagination.SortBy?.ToLower() switch
        {
            "name"       => pagination.SortDescending ? query.OrderByDescending(e => e.FirstName) : query.OrderBy(e => e.FirstName),
            "code"       => pagination.SortDescending ? query.OrderByDescending(e => e.EmployeeCode) : query.OrderBy(e => e.EmployeeCode),
            "joiningdate"=> pagination.SortDescending ? query.OrderByDescending(e => e.JoiningDate) : query.OrderBy(e => e.JoiningDate),
            _            => query.OrderBy(e => e.EmployeeCode)
        };

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
        => await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Designation)
            .Where(e => e.DepartmentId == departmentId && e.IsActive)
            .OrderBy(e => e.FirstName)
            .ToListAsync();

    public async Task<IEnumerable<Employee>> GetByManagerAsync(int managerId)
        => await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Where(e => e.ManagerId == managerId && e.IsActive)
            .OrderBy(e => e.FirstName)
            .ToListAsync();

    public async Task<bool> EmployeeCodeExistsAsync(string code)
        => await _context.Employees.AnyAsync(e => e.EmployeeCode == code);

    public async Task<string> GenerateEmployeeCodeAsync()
    {
        var count = await _context.Employees.CountAsync();
        return $"EMP{(count + 1):D5}";
    }
}

// ─── DepartmentRepository ─────────────────────────────────────────────────────
public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(EasyPayDbContext context) : base(context) { }

    public override async Task<IEnumerable<Department>> GetAllAsync()
        => await _context.Departments
            .Include(d => d.Employees)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

    public async Task<Department?> GetWithEmployeesAsync(int departmentId)
        => await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);

    public async Task<bool> DepartmentCodeExistsAsync(string code)
        => await _context.Departments.AnyAsync(d => d.DepartmentCode == code);

    public async Task<IEnumerable<Department>> GetActiveAsync()
        => await _context.Departments
            .Include(d => d.Employees)
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
}

// ─── DesignationRepository ────────────────────────────────────────────────────
public class DesignationRepository : GenericRepository<Designation>, IDesignationRepository
{
    public DesignationRepository(EasyPayDbContext context) : base(context) { }

    public async Task<IEnumerable<Designation>> GetByDepartmentAsync(int departmentId)
        => await _context.Designations
            .Where(d => d.DepartmentId == departmentId && d.IsActive)
            .OrderBy(d => d.DesignationName)
            .ToListAsync();

    public async Task<bool> DesignationCodeExistsAsync(string code)
        => await _context.Designations.AnyAsync(d => d.DesignationCode == code);

    public async Task<IEnumerable<Designation>> GetActiveAsync()
        => await _context.Designations
            .Include(d => d.Department)
            .Where(d => d.IsActive)
            .OrderBy(d => d.DesignationName)
            .ToListAsync();
}

// ─── PayrollPolicyRepository ──────────────────────────────────────────────────
public class PayrollPolicyRepository : GenericRepository<PayrollPolicy>, IPayrollPolicyRepository
{
    public PayrollPolicyRepository(EasyPayDbContext context) : base(context) { }

    public async Task<PayrollPolicy?> GetActivePolicyAsync()
        => await _context.PayrollPolicies
            .Where(p => p.IsActive && p.EffectiveTo == null)
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<PayrollPolicy>> GetActiveAsync()
        => await _context.PayrollPolicies
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.EffectiveFrom)
            .ToListAsync();
}

// ─── SalaryStructureRepository ────────────────────────────────────────────────
public class SalaryStructureRepository : GenericRepository<SalaryStructure>, ISalaryStructureRepository
{
    public SalaryStructureRepository(EasyPayDbContext context) : base(context) { }

    public async Task<SalaryStructure?> GetCurrentForEmployeeAsync(int employeeId)
        => await _context.SalaryStructures
            .Include(s => s.Policy)
            .Include(s => s.Employee).ThenInclude(e => e.User)
            .Where(s => s.EmployeeId == employeeId && s.IsActive)
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<SalaryStructure>> GetHistoryForEmployeeAsync(int employeeId)
        => await _context.SalaryStructures
            .Include(s => s.Policy)
            .Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.EffectiveFrom)
            .ToListAsync();

    public async Task DeactivatePreviousAsync(int employeeId)
    {
        await _context.SalaryStructures
            .Where(s => s.EmployeeId == employeeId && s.IsActive)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsActive, false)
                .SetProperty(x => x.EffectiveTo, DateOnly.FromDateTime(DateTime.UtcNow))
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    }
}

// ─── PayrollRepository ────────────────────────────────────────────────────────
public class PayrollRepository : GenericRepository<Payroll>, IPayrollRepository
{
    public PayrollRepository(EasyPayDbContext context) : base(context) { }

    public async Task<Payroll?> GetWithDetailsAsync(int payrollId)
        => await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.User)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Policy)
            .Include(p => p.SalaryStructure)
            .FirstOrDefaultAsync(p => p.PayrollId == payrollId);

    public async Task<(IEnumerable<Payroll> Items, int Total)> GetPagedAsync(
        PaginationParams pagination, int? employeeId = null, string? status = null,
        int? year = null, int? month = null)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.User)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .AsQueryable();

        if (employeeId.HasValue)  query = query.Where(p => p.EmployeeId == employeeId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        if (year.HasValue)  query = query.Where(p => p.PayPeriodStart.Year == year.Value);
        if (month.HasValue) query = query.Where(p => p.PayPeriodStart.Month == month.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.PayPeriodStart)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Payroll?> GetForPeriodAsync(int employeeId, DateOnly periodStart, DateOnly periodEnd)
        => await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.User)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Policy)
            .Include(p => p.SalaryStructure)
            .FirstOrDefaultAsync(p =>
                p.EmployeeId == employeeId &&
                p.PayPeriodStart == periodStart &&
                p.PayPeriodEnd == periodEnd);

    public async Task<IEnumerable<Payroll>> GetByStatusAsync(string status)
        => await _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.User)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.PayPeriodStart)
            .ToListAsync();

    public async Task<IEnumerable<Payroll>> GetForReportAsync(DateOnly periodStart, DateOnly periodEnd, string? status = null)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee).ThenInclude(e => e.User)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Where(p => p.PayPeriodStart >= periodStart && p.PayPeriodEnd <= periodEnd);
            
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);
            
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Payroll>> GetForBulkProcessingAsync(
        DateOnly periodStart, DateOnly periodEnd)
    {
        var activeIds = await _context.Employees
            .Where(e => e.IsActive && e.EmploymentStatus == "Active")
            .Select(e => e.EmployeeId)
            .ToListAsync();

        return await _context.Payrolls
            .Where(p => activeIds.Contains(p.EmployeeId) &&
                        p.PayPeriodStart == periodStart &&
                        p.PayPeriodEnd == periodEnd)
            .ToListAsync();
    }

    public async Task<bool> PayrollExistsForPeriodAsync(
        int employeeId, DateOnly periodStart, DateOnly periodEnd)
        => await _context.Payrolls.AnyAsync(p =>
            p.EmployeeId == employeeId &&
            p.PayPeriodStart == periodStart &&
            p.PayPeriodEnd == periodEnd &&
            p.Status != "Cancelled");
}

// ─── LeaveRequestRepository ───────────────────────────────────────────────────
public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(EasyPayDbContext context) : base(context) { }

    public async Task<(IEnumerable<LeaveRequest> Items, int Total)> GetPagedAsync(
        PaginationParams pagination, int? employeeId = null, string? status = null)
    {
        var query = _context.LeaveRequests
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Include(l => l.Employee).ThenInclude(e => e.Department)
            .Include(l => l.LeaveType)
            .Include(l => l.ApprovedBy)
            .AsQueryable();

        if (employeeId.HasValue) query = query.Where(l => l.EmployeeId == employeeId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(l => l.Status == status);

        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var term = pagination.SearchTerm.ToLower();
            query = query.Where(l =>
                l.Employee.FirstName.ToLower().Contains(term) ||
                l.Employee.LastName.ToLower().Contains(term)  ||
                l.LeaveType.LeaveTypeName.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingForManagerAsync(int managerId)
        => await _context.LeaveRequests
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Include(l => l.Employee).ThenInclude(e => e.Department)
            .Include(l => l.LeaveType)
            .Where(l => l.Employee.ManagerId == managerId && l.Status == "Pending")
            .OrderBy(l => l.FromDate)
            .ToListAsync();

    public async Task<int> GetUsedLeaveDaysAsync(int employeeId, int leaveTypeId, int year)
        => await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId &&
                        l.LeaveTypeId == leaveTypeId &&
                        l.FromDate.Year == year &&
                        l.Status == "Approved")
            .SumAsync(l => l.TotalDays);

    public async Task<bool> HasOverlappingLeaveAsync(
        int employeeId, DateOnly fromDate, DateOnly toDate, int? excludeId = null)
    {
        var query = _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId &&
                        l.Status != "Cancelled" && l.Status != "Rejected" &&
                        l.FromDate <= toDate && l.ToDate >= fromDate);

        if (excludeId.HasValue)
            query = query.Where(l => l.LeaveRequestId != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAndYearAsync(int employeeId, int year)
        => await _context.LeaveRequests
            .Include(l => l.LeaveType)
            .Where(l => l.EmployeeId == employeeId && l.FromDate.Year == year)
            .OrderByDescending(l => l.FromDate)
            .ToListAsync();
}

// ─── TimesheetRepository ──────────────────────────────────────────────────────
public class TimesheetRepository : GenericRepository<Timesheet>, ITimesheetRepository
{
    public TimesheetRepository(EasyPayDbContext context) : base(context) { }

    public async Task<(IEnumerable<Timesheet> Items, int Total)> GetPagedAsync(
        PaginationParams pagination, int? employeeId = null,
        DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var query = _context.Timesheets
            .Include(t => t.Employee).ThenInclude(e => e.User)
            .AsQueryable();

        if (employeeId.HasValue)  query = query.Where(t => t.EmployeeId == employeeId.Value);
        if (fromDate.HasValue)    query = query.Where(t => t.WorkDate >= fromDate.Value);
        if (toDate.HasValue)      query = query.Where(t => t.WorkDate <= toDate.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.WorkDate)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Timesheet?> GetForDateAsync(int employeeId, DateOnly workDate)
        => await _context.Timesheets
            .FirstOrDefaultAsync(t => t.EmployeeId == employeeId && t.WorkDate == workDate);

    public async Task<IEnumerable<Timesheet>> GetForPayrollPeriodAsync(
        int employeeId, DateOnly start, DateOnly end)
        => await _context.Timesheets
            .Where(t => t.EmployeeId == employeeId &&
                        t.WorkDate >= start && t.WorkDate <= end &&
                        t.Status == "Approved")
            .ToListAsync();

    public async Task<decimal> GetTotalHoursAsync(int employeeId, DateOnly start, DateOnly end)
        => await _context.Timesheets
            .Where(t => t.EmployeeId == employeeId &&
                        t.WorkDate >= start && t.WorkDate <= end &&
                        t.Status == "Approved")
            .SumAsync(t => t.HoursWorked);

    public async Task<decimal> GetTotalOvertimeAsync(int employeeId, DateOnly start, DateOnly end)
        => await _context.Timesheets
            .Where(t => t.EmployeeId == employeeId &&
                        t.WorkDate >= start && t.WorkDate <= end &&
                        t.Status == "Approved")
            .SumAsync(t => t.OvertimeHours);
}

// ─── BenefitRepository ────────────────────────────────────────────────────────
public class BenefitRepository : GenericRepository<Benefit>, IBenefitRepository
{
    public BenefitRepository(EasyPayDbContext context) : base(context) { }

    public async Task<IEnumerable<Benefit>> GetActiveAsync()
        => await _context.Benefits
            .Where(b => b.IsActive)
            .OrderBy(b => b.BenefitName)
            .ToListAsync();

    public async Task<bool> BenefitCodeExistsAsync(string code)
        => await _context.Benefits.AnyAsync(b => b.BenefitCode == code);
}

// ─── EmployeeBenefitRepository ────────────────────────────────────────────────
public class EmployeeBenefitRepository : GenericRepository<EmployeeBenefit>, IEmployeeBenefitRepository
{
    public EmployeeBenefitRepository(EasyPayDbContext context) : base(context) { }

    public async Task<IEnumerable<EmployeeBenefit>> GetActiveForEmployeeAsync(int employeeId)
        => await _context.EmployeeBenefits
            .Include(eb => eb.Benefit)
            .Include(eb => eb.Employee).ThenInclude(e => e.User)
            .Where(eb => eb.EmployeeId == employeeId && eb.IsActive)
            .ToListAsync();

    public async Task DeactivatePreviousAsync(int employeeId, int benefitId)
    {
        await _context.EmployeeBenefits
            .Where(eb => eb.EmployeeId == employeeId &&
                         eb.BenefitId == benefitId && eb.IsActive)
            .ExecuteUpdateAsync(eb => eb
                .SetProperty(x => x.IsActive, false)
                .SetProperty(x => x.EffectiveTo, DateOnly.FromDateTime(DateTime.UtcNow))
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    }
}

// ─── LeaveTypeRepository ──────────────────────────────────────────────────────
public class LeaveTypeRepository : GenericRepository<LeaveType>, ILeaveTypeRepository
{
    public LeaveTypeRepository(EasyPayDbContext context) : base(context) { }

    public async Task<IEnumerable<LeaveType>> GetActiveAsync()
        => await _context.LeaveTypes
            .Where(lt => lt.IsActive)
            .OrderBy(lt => lt.LeaveTypeName)
            .ToListAsync();

    public async Task<LeaveType?> GetByCodeAsync(string code)
        => await _context.LeaveTypes.FirstOrDefaultAsync(lt => lt.LeaveCode == code);
}
