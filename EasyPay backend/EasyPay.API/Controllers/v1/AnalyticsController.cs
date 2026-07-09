using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

/// <summary>
/// Analytics module — headcount, leave analytics.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly ILeaveService        _leaveService;
    private readonly IEmployeeRepository  _employeeRepo;
    private readonly IDepartmentRepository _deptRepo;

    public AnalyticsController(
        ILeaveService        leaveService,
        IEmployeeRepository  employeeRepo,
        IDepartmentRepository deptRepo)
    {
        _leaveService   = leaveService;
        _employeeRepo   = employeeRepo;
        _deptRepo       = deptRepo;
    }

    /// <summary>Headcount report — active employees grouped by department.</summary>
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

    /// <summary>Leave utilisation report — approved leave days by type for a year.</summary>
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
}
