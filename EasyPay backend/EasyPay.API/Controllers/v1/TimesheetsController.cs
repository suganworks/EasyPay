using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Benefit;
using EasyPay.Core.DTOs.Timesheet;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

// ─── TimesheetsController ──────────────────────────────────────────────────────

/// <summary>Submit, update, approve, and retrieve timesheets.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class TimesheetsController : ControllerBase
{
    private readonly ITimesheetService _service;
    private readonly ICurrentUserService _currentUser;

    public TimesheetsController(ITimesheetService service, ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    /// <summary>Get paged timesheets (admin/HR/manager view).</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] int? employeeId = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var result = await _service.GetPagedAsync(pagination, employeeId, fromDate, toDate);
        return Ok(result);
    }

    /// <summary>Get my timesheets (employee self-service).</summary>
    [HttpGet("my-timesheets")]
    public async Task<IActionResult> GetMine(
        [FromQuery] PaginationParams pagination,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException(AppConstants.ErrorMessages.EmployeeProfileNotFound);
        var result = await _service.GetPagedAsync(pagination, empId, fromDate, toDate);
        return Ok(result);
    }

    /// <summary>Get a single timesheet by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (_currentUser.IsInRole(AppConstants.Roles.Employee) &&
            result.EmployeeId != _currentUser.EmployeeId)
            return Forbid();

        return Ok(ApiResponse<TimesheetResponseDto>.SuccessResponse(result));
    }

    /// <summary>Submit a timesheet entry.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TimesheetResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTimesheetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException(AppConstants.ErrorMessages.EmployeeProfileNotFound);

        var result = await _service.CreateAsync(empId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.TimesheetId },
            ApiResponse<TimesheetResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.Created));
    }

    /// <summary>Update a pending timesheet.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTimesheetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException(AppConstants.ErrorMessages.EmployeeProfileNotFound);

        var result = await _service.UpdateAsync(id, empId, dto);
        return Ok(ApiResponse<TimesheetResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.Updated));
    }

    /// <summary>Approve or reject a timesheet (manager/HR/admin).</summary>
    [HttpPatch("{id:int}/action")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> ApproveOrReject(int id, [FromBody] ApproveTimesheetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var approverId = _currentUser.EmployeeId ?? 0;

        var result = await _service.ApproveOrRejectAsync(id, approverId, dto);
        return Ok(ApiResponse<TimesheetResponseDto>.SuccessResponse(
            result, $"Timesheet {result.Status.ToLower()} successfully."));
    }

    /// <summary>Bulk approve multiple timesheets at once.</summary>
    [HttpPost("bulk-approve")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> BulkApprove([FromBody] BulkApproveTimesheetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var approverId = _currentUser.EmployeeId ?? 0;
        var count      = await _service.BulkApproveAsync(dto.TimesheetIds, approverId);
        return Ok(ApiResponse<object>.SuccessResponse(
            new { ApprovedCount = count },
            $"{count} timesheet(s) approved successfully."));
    }

    /// <summary>Get monthly timesheet summary for an employee.</summary>
    [HttpGet("monthly-summary/{employeeId:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.Manager},{AppConstants.Roles.PayrollProcessor}")]
    public async Task<IActionResult> MonthlySummary(
        int employeeId,
        [FromQuery] int? year  = null,
        [FromQuery] int? month = null)
    {
        var result = await _service.GetMonthlySummaryAsync(
            employeeId,
            year  ?? DateTime.UtcNow.Year,
            month ?? DateTime.UtcNow.Month);
        return Ok(ApiResponse<TimesheetMonthlySummaryDto>.SuccessResponse(result));
    }

    /// <summary>Get my monthly timesheet summary (employee self-service).</summary>
    [HttpGet("my-monthly-summary")]
    public async Task<IActionResult> MyMonthlySummary(
        [FromQuery] int? year  = null,
        [FromQuery] int? month = null)
    {
        var empId = _currentUser.EmployeeId
            ?? throw new Core.Exceptions.UnauthorizedException(AppConstants.ErrorMessages.EmployeeProfileNotFound);
        var result = await _service.GetMonthlySummaryAsync(
            empId,
            year  ?? DateTime.UtcNow.Year,
            month ?? DateTime.UtcNow.Month);
        return Ok(ApiResponse<TimesheetMonthlySummaryDto>.SuccessResponse(result));
    }
}
