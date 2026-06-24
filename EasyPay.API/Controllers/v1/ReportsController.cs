using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Payroll;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor}")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IPayrollService      _payrollService;
    private readonly IPayrollRepository   _payrollRepo;
    private readonly ILeaveService        _leaveService;
    private readonly IEmployeeRepository  _employeeRepo;
    private readonly IDepartmentRepository _deptRepo;

    public ReportsController(
        IPayrollService      payrollService,
        IPayrollRepository   payrollRepo,
        ILeaveService        leaveService,
        IEmployeeRepository  employeeRepo,
        IDepartmentRepository deptRepo)
    {
        _payrollService = payrollService;
        _payrollRepo    = payrollRepo;
        _leaveService   = leaveService;
        _employeeRepo   = employeeRepo;
        _deptRepo       = deptRepo;
    }

    [HttpGet("payroll-register")]
    public async Task<IActionResult> PayrollRegister(
        [FromQuery] DateOnly periodStart,
        [FromQuery] DateOnly periodEnd,
        [FromQuery] string?  status = null)
    {
        var pagination = new PaginationParams { PageNumber = 1, PageSize = AppConstants.Pagination.MaxPageSize };
        var (items, total) = await _payrollRepo.GetPagedAsync(pagination, null, status, null, null);

        var filtered = items
            .Where(p => p.PayPeriodStart == periodStart && p.PayPeriodEnd == periodEnd)
            .ToList();

        var report = new
        {
            PayPeriod        = $"{periodStart:dd MMM yyyy} – {periodEnd:dd MMM yyyy}",
            GeneratedAt      = DateTime.UtcNow,
            TotalRecords     = filtered.Count,
            TotalGross       = filtered.Sum(p => p.GrossEarnings),
            TotalDeductions  = filtered.Sum(p => p.TotalDeductions),
            TotalNet         = filtered.Sum(p => p.NetSalary),
            TotalPfEmployee  = filtered.Sum(p => p.PfEmployee),
            TotalPfEmployer  = filtered.Sum(p => p.PfEmployer),
            TotalEsiEmployee = filtered.Sum(p => p.EsiEmployee),
            TotalEsiEmployer = filtered.Sum(p => p.EsiEmployer),
            TotalProfTax     = filtered.Sum(p => p.ProfessionalTax),
            TotalIncomeTax   = filtered.Sum(p => p.IncomeTax),
            Records          = filtered.Select(p => new
            {
                p.PayrollId,
                p.EmployeeId,
                EmployeeName    = p.Employee?.FullName ?? string.Empty,
                EmployeeCode    = p.Employee?.EmployeeCode ?? string.Empty,
                DepartmentName  = p.Employee?.Department?.DepartmentName ?? string.Empty,
                p.GrossEarnings,
                p.TotalDeductions,
                p.NetSalary,
                p.PfEmployee,
                p.PfEmployer,
                p.EsiEmployee,
                p.EsiEmployer,
                p.ProfessionalTax,
                p.IncomeTax,
                p.Status,
                p.PaymentDate
            })
        };

        return Ok(ApiResponse<object>.SuccessResponse(report, "Payroll register generated."));
    }

    [HttpGet("headcount")]
    public async Task<IActionResult> Headcount()
    {
        var departments = await _deptRepo.GetActiveAsync();
        var breakdown   = new List<object>();

        foreach (var dept in departments)
        {
            var employees = (await _employeeRepo.GetByDepartmentAsync(dept.DepartmentId)).ToList();
            breakdown.Add(new
            {
                DepartmentId   = dept.DepartmentId,
                DepartmentName = dept.DepartmentName,
                TotalActive    = employees.Count(e => e.EmploymentStatus == "Active"),
                FullTime       = employees.Count(e => e.EmploymentType == "FullTime"),
                PartTime       = employees.Count(e => e.EmploymentType == "PartTime"),
                Contract       = employees.Count(e => e.EmploymentType == "Contract"),
                Intern         = employees.Count(e => e.EmploymentType == "Intern")
            });
        }

        var allActive = await _employeeRepo.FindAsync(e => e.IsActive && e.EmploymentStatus == "Active");

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            GeneratedAt  = DateTime.UtcNow,
            TotalActive  = allActive.Count(),
            DepartmentBreakdown = breakdown
        }, "Headcount report generated."));
    }

    [HttpGet("leave-utilisation")]
    public async Task<IActionResult> LeaveUtilisation(
        [FromQuery] int? year = null,
        [FromQuery] int? departmentId = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var pagination = new PaginationParams { PageNumber = 1, PageSize = AppConstants.Pagination.MaxPageSize };

        var allLeaves = await _leaveService.GetPagedAsync(pagination, null, "Approved");
        var filtered  = allLeaves.Data.Where(l => l.FromDate.Year == targetYear).ToList();

        if (departmentId.HasValue)
        {
            var deptEmps   = await _employeeRepo.GetByDepartmentAsync(departmentId.Value);
            var deptEmpIds = deptEmps.Select(e => e.EmployeeId).ToHashSet();
            filtered       = filtered.Where(l => deptEmpIds.Contains(l.EmployeeId)).ToList();
        }

        var byType = filtered
            .GroupBy(l => l.LeaveTypeName)
            .Select(g => new { LeaveType = g.Key, TotalRequests = g.Count(), TotalDays = g.Sum(l => l.TotalDays) })
            .OrderByDescending(x => x.TotalDays)
            .ToList();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            Year         = targetYear,
            DepartmentId = departmentId,
            GeneratedAt  = DateTime.UtcNow,
            TotalRequests= filtered.Count,
            TotalDays    = filtered.Sum(l => l.TotalDays),
            ByLeaveType  = byType
        }, "Leave utilisation report generated."));
    }

    [HttpGet("ctc-summary")]
    public async Task<IActionResult> CtcSummary(
        [FromQuery] DateOnly periodStart,
        [FromQuery] DateOnly periodEnd)
    {
        var (items, _) = await _payrollRepo.GetPagedAsync(
            new PaginationParams { PageNumber = 1, PageSize = AppConstants.Pagination.MaxPageSize },
            null, "Approved", null, null);

        var filtered = items
            .Where(p => p.PayPeriodStart >= periodStart && p.PayPeriodEnd <= periodEnd)
            .ToList();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            PayPeriod        = $"{periodStart:dd MMM yyyy} – {periodEnd:dd MMM yyyy}",
            GeneratedAt      = DateTime.UtcNow,
            TotalEmployees   = filtered.Select(p => p.EmployeeId).Distinct().Count(),
            TotalGross       = filtered.Sum(p => p.GrossEarnings),
            TotalPfEmployer  = filtered.Sum(p => p.PfEmployer),
            TotalEsiEmployer = filtered.Sum(p => p.EsiEmployer),
            TotalCTC         = filtered.Sum(p => p.GrossEarnings + p.PfEmployer + p.EsiEmployer),
            Records          = filtered.Select(p => new
            {
                p.EmployeeId,
                EmployeeName   = p.Employee?.FullName ?? string.Empty,
                EmployeeCode   = p.Employee?.EmployeeCode ?? string.Empty,
                DepartmentName = p.Employee?.Department?.DepartmentName ?? string.Empty,
                p.GrossEarnings,
                p.PfEmployer,
                p.EsiEmployer,
                CTC = p.GrossEarnings + p.PfEmployer + p.EsiEmployer
            })
        }, "CTC summary generated."));
    }

    [HttpGet("payroll-status-dashboard")]
    public async Task<IActionResult> PayrollStatusDashboard(
        [FromQuery] int? year  = null,
        [FromQuery] int? month = null)
    {
        var targetYear  = year  ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        var (items, total) = await _payrollRepo.GetPagedAsync(
            new PaginationParams { PageNumber = 1, PageSize = AppConstants.Pagination.MaxPageSize },
            null, null, targetYear, targetMonth);

        var byStatus = items
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), NetTotal = g.Sum(p => p.NetSalary) })
            .ToList();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            Year       = targetYear,
            Month      = targetMonth,
            TotalCount = total,
            ByStatus   = byStatus
        }));
    }
}
