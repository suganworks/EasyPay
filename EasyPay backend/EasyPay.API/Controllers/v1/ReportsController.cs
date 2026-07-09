using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Payroll;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

/// <summary>
/// Reporting module — payroll summaries, headcount, leave analytics, pay-register.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor}")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IPayrollRepository   _payrollRepo;
    public ReportsController(IPayrollRepository payrollRepo)
    {
        _payrollRepo = payrollRepo;
    }

    /// <summary>Payroll register for a given pay period — queries DB directly.</summary>
    [HttpGet("payroll-register")]
    public async Task<IActionResult> PayrollRegister(
        [FromQuery] DateOnly periodStart,
        [FromQuery] DateOnly periodEnd,
        [FromQuery] string?  status = null)
    {
        // Direct DB query — no in-memory filter, proper pagination
        var filtered = (await _payrollRepo.GetForReportAsync(periodStart, periodEnd, status)).ToList();

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


    /// <summary>CTC summary — gross + employer contributions.</summary>
    [HttpGet("ctc-summary")]
    public async Task<IActionResult> CtcSummary(
        [FromQuery] DateOnly periodStart,
        [FromQuery] DateOnly periodEnd)
    {
        var allRecords = await _payrollRepo.GetForReportAsync(periodStart, periodEnd, null);
        var filtered = allRecords.Where(p => p.Status == "Approved" || p.Status == "Paid").ToList();

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

    /// <summary>Payroll status dashboard — count by status for a period.</summary>
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
