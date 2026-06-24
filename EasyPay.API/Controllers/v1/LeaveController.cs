using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Leave;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (_currentUser.IsInRole(AppConstants.Roles.Employee) &&
            result.EmployeeId != _currentUser.EmployeeId)
            return Forbid();

        return Ok(ApiResponse<LeaveRequestResponseDto>.SuccessResponse(result));
    }

    [HttpGet("my-balance")]
    public async Task<IActionResult> GetMyBalance([FromQuery] int? year = null)
    {
        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException("Employee profile not found.");
        var targetYear = year ?? DateTime.UtcNow.Year;
        var result = await _service.GetBalancesAsync(empId, targetYear);
        return Ok(ApiResponse<IEnumerable<LeaveBalanceDto>>.SuccessResponse(result));
    }

    [HttpGet("{employeeId:int}/balance")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetBalance(int employeeId, [FromQuery] int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var result = await _service.GetBalancesAsync(employeeId, targetYear);
        return Ok(ApiResponse<IEnumerable<LeaveBalanceDto>>.SuccessResponse(result));
    }

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

    [HttpPatch("{id:int}/action")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> ApproveOrReject(int id, [FromBody] ApproveLeaveDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var approverId = _currentUser.EmployeeId ?? 0;

        var result = await _service.ApproveOrRejectAsync(id, approverId, dto);
        return Ok(ApiResponse<LeaveRequestResponseDto>.SuccessResponse(
            result, $"Leave request {result.Status.ToLower()} successfully."));
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException("Employee profile not found.");

        var result = await _service.CancelAsync(id, empId);
        return Ok(ApiResponse<LeaveRequestResponseDto>.SuccessResponse(result, "Leave request cancelled."));
    }

    [HttpPost("carry-forward/{employeeId:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> CarryForward(int employeeId, [FromQuery] int? fromYear = null)
    {
        var result = await _service.ProcessCarryForwardAsync(
            employeeId, fromYear ?? DateTime.UtcNow.Year - 1);
        return Ok(ApiResponse<LeaveCarryForwardDto>.SuccessResponse(
            result, "Carry-forward processed successfully."));
    }

    [HttpPost("carry-forward/bulk")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> BulkCarryForward([FromQuery] int? fromYear = null)
    {
        var results = await _service.BulkCarryForwardAsync(fromYear ?? DateTime.UtcNow.Year - 1);
        return Ok(ApiResponse<IEnumerable<LeaveCarryForwardDto>>.SuccessResponse(
            results, $"Carry-forward processed for {results.Count()} employees."));
    }
}
