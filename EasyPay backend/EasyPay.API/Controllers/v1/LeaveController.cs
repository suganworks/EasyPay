using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Leave;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

/// <summary>Submit, approve, reject, and manage leave requests.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _service;
    private readonly ICurrentUserService _currentUser;

    public LeaveController(ILeaveService service, ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    /// <summary>Get paged leave requests (HR/Admin/Manager view).</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] int? employeeId = null,
        [FromQuery] string? status = null)
    {
        var result = await _service.GetPagedAsync(pagination, employeeId, status);
        return Ok(result);
    }

    /// <summary>Get my leave requests (employee self-service).</summary>
    [HttpGet("my-leaves")]
    public async Task<IActionResult> GetMyLeaves(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? status = null)
    {
        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException("Employee profile not found.");
        var result = await _service.GetPagedAsync(pagination, empId, status);
        return Ok(result);
    }

    /// <summary>Get pending leave requests for the logged-in manager to action.</summary>
    [HttpGet("pending-for-me")]
    [Authorize(Roles = $"{AppConstants.Roles.Manager},{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> GetPendingForMe()
    {
        var managerId = _currentUser.EmployeeId ?? 0;
        if (managerId == 0)
            return Ok(ApiResponse<IEnumerable<LeaveRequestResponseDto>>.SuccessResponse(
                Enumerable.Empty<LeaveRequestResponseDto>(),
                "No employee profile linked to this account."));
        var result = await _service.GetPendingForManagerAsync(managerId);
        return Ok(ApiResponse<IEnumerable<LeaveRequestResponseDto>>.SuccessResponse(result));
    }

    /// <summary>Get leave request by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        // Employee can only see their own leave
        if (_currentUser.IsInRole(AppConstants.Roles.Employee) &&
            result.EmployeeId != _currentUser.EmployeeId)
            return Forbid();

        return Ok(ApiResponse<LeaveRequestResponseDto>.SuccessResponse(result));
    }

    /// <summary>Get leave balances for the logged-in employee.</summary>
    [HttpGet("my-balance")]
    public async Task<IActionResult> GetMyBalance([FromQuery] int? year = null)
    {
        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException("Employee profile not found.");
        var targetYear = year ?? DateTime.UtcNow.Year;
        var result = await _service.GetBalancesAsync(empId, targetYear);
        return Ok(ApiResponse<IEnumerable<LeaveBalanceDto>>.SuccessResponse(result));
    }

    /// <summary>Get leave balances for a specific employee (HR/Admin).</summary>
    [HttpGet("{employeeId:int}/balance")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetBalance(int employeeId, [FromQuery] int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var result = await _service.GetBalancesAsync(employeeId, targetYear);
        return Ok(ApiResponse<IEnumerable<LeaveBalanceDto>>.SuccessResponse(result));
    }

    /// <summary>Submit a leave request.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LeaveRequestResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Submit([FromBody] CreateLeaveRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException("Employee profile not found.");

        var result = await _service.SubmitAsync(empId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.LeaveRequestId },
            ApiResponse<LeaveRequestResponseDto>.SuccessResponse(result, "Leave request submitted successfully."));
    }

    /// <summary>Approve or reject a leave request.</summary>
    [HttpPatch("{id:int}/action")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> ApproveOrReject(int id, [FromBody] ApproveLeaveDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        // Admin may not have an employee profile — use 0 as a placeholder approverId
        var approverId = _currentUser.EmployeeId ?? 0;

        var result = await _service.ApproveOrRejectAsync(id, approverId, dto);
        return Ok(ApiResponse<LeaveRequestResponseDto>.SuccessResponse(
            result, $"Leave request {result.Status.ToLower()} successfully."));
    }

    /// <summary>Cancel a leave request (employee can cancel their own).</summary>
    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException("Employee profile not found.");

        var result = await _service.CancelAsync(id, empId);
        return Ok(ApiResponse<LeaveRequestResponseDto>.SuccessResponse(result, "Leave request cancelled."));
    }

    /// <summary>Process leave carry-forward for a single employee at year end.</summary>
    [HttpPost("carry-forward/{employeeId:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> CarryForward(int employeeId, [FromQuery] int? fromYear = null)
    {
        var result = await _service.ProcessCarryForwardAsync(
            employeeId, fromYear ?? DateTime.UtcNow.Year - 1);
        return Ok(ApiResponse<LeaveCarryForwardDto>.SuccessResponse(
            result, "Carry-forward processed successfully."));
    }

    /// <summary>Bulk carry-forward for ALL active employees at year end.</summary>
    [HttpPost("carry-forward/bulk")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> BulkCarryForward([FromQuery] int? fromYear = null)
    {
        var results = await _service.BulkCarryForwardAsync(fromYear ?? DateTime.UtcNow.Year - 1);
        return Ok(ApiResponse<IEnumerable<LeaveCarryForwardDto>>.SuccessResponse(
            results, $"Carry-forward processed for {results.Count()} employees."));
    }
}
