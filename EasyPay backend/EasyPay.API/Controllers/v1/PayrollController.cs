using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Payroll;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

// ─── PayrollController ────────────────────────────────────────────────────────

/// <summary>Process, approve, and retrieve payroll records.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _service;
    private readonly ICurrentUserService _currentUser;

    public PayrollController(IPayrollService service, ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    /// <summary>Get paged payroll records with optional filters.</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor}")]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] int? employeeId = null,
        [FromQuery] string? status = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var result = await _service.GetPagedAsync(pagination, employeeId, status, year, month);
        return Ok(result);
    }

    /// <summary>Get my payroll records (for logged-in employee).</summary>
    [HttpGet("my-payrolls")]
    [Authorize(Roles = AppConstants.Roles.Employee)]
    public async Task<IActionResult> GetMyPayrolls(
        [FromQuery] PaginationParams pagination,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var empId = _currentUser.EmployeeId
            ?? throw new Core.Exceptions.UnauthorizedException("Employee profile not found.");
        var result = await _service.GetPagedAsync(pagination, empId, null, year, month);
        return Ok(result);
    }

    /// <summary>Get payroll by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        // Employees can only view their own payroll
        if (_currentUser.IsInRole(AppConstants.Roles.Employee) &&
            result.EmployeeId != _currentUser.EmployeeId)
            return Forbid();

        return Ok(ApiResponse<PayrollResponseDto>.SuccessResponse(result));
    }

    /// <summary>Get payroll summary for a pay period.</summary>
    [HttpGet("summary")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor}")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateOnly periodStart, [FromQuery] DateOnly periodEnd)
    {
        var result = await _service.GetSummaryAsync(periodStart, periodEnd);
        return Ok(ApiResponse<PayrollSummaryDto>.SuccessResponse(result));
    }

    /// <summary>Process payroll for a single employee.</summary>
    [HttpPost("process")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor}")]
    [ProducesResponseType(typeof(ApiResponse<PayrollResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Process([FromBody] ProcessPayrollDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var result = await _service.ProcessAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.PayrollId },
            ApiResponse<PayrollResponseDto>.SuccessResponse(result, "Payroll processed successfully."));
    }

    /// <summary>Bulk process payroll for all or selected employees.</summary>
    [HttpPost("bulk-process")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor}")]
    public async Task<IActionResult> BulkProcess([FromBody] BulkProcessPayrollDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var results = await _service.BulkProcessAsync(dto);
        return Ok(ApiResponse<IEnumerable<PayrollResponseDto>>.SuccessResponse(
            results, $"Bulk payroll processed for {results.Count()} employees."));
    }

    /// <summary>Approve a pending payroll.</summary>
    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _service.ApproveAsync(id);
        return Ok(ApiResponse<PayrollResponseDto>.SuccessResponse(result, "Payroll approved."));
    }

    /// <summary>Reject/cancel a pending payroll.</summary>
    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectPayrollDto dto)
    {
        var result = await _service.RejectAsync(id, dto.Remarks);
        return Ok(ApiResponse<PayrollResponseDto>.SuccessResponse(result, "Payroll rejected."));
    }

    /// <summary>Mark an approved payroll as Paid after salary disbursement.</summary>
    [HttpPatch("{id:int}/mark-as-paid")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor}")]
    public async Task<IActionResult> MarkAsPaid(int id, [FromBody] MarkAsPaidDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));
        var result = await _service.MarkAsPaidAsync(id, dto.PaymentDate);
        return Ok(ApiResponse<PayrollResponseDto>.SuccessResponse(result, "Payroll marked as paid."));
    }
}

// ─── Inline DTO for reject ────────────────────────────────────────────────────
public class RejectPayrollDto
{
    public string Remarks { get; set; } = string.Empty;
}

// ─── PayrollPoliciesController ────────────────────────────────────────────────

/// <summary>Manage payroll policies (PF, ESI, PT rates, working days, etc.).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payroll-policies")]
[Authorize]
[Produces("application/json")]
public class PayrollPoliciesController : ControllerBase
{
    private readonly IPayrollPolicyService _service;

    public PayrollPoliciesController(IPayrollPolicyService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<IEnumerable<PayrollPolicyResponseDto>>.SuccessResponse(
            await _service.GetAllAsync()));

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
        => Ok(ApiResponse<PayrollPolicyResponseDto?>.SuccessResponse(
            await _service.GetActiveAsync()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<PayrollPolicyResponseDto>.SuccessResponse(
            await _service.GetByIdAsync(id)));

    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Create([FromBody] CreatePayrollPolicyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.PolicyId },
            ApiResponse<PayrollPolicyResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.Created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePayrollPolicyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));
        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<PayrollPolicyResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.Updated));
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);
        return Ok(ApiResponse.SuccessResponse("Policy deactivated."));
    }
}

// ─── SalaryStructuresController ───────────────────────────────────────────────

/// <summary>Manage employee salary structures (CTC breakup).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/salary-structures")]
[Authorize]
[Produces("application/json")]
public class SalaryStructuresController : ControllerBase
{
    private readonly ISalaryStructureService _service;
    private readonly ICurrentUserService _currentUser;

    public SalaryStructuresController(ISalaryStructureService service, ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    /// <summary>Assign / update salary structure for an employee.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Create([FromBody] CreateSalaryStructureDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));
        var result = await _service.CreateAsync(dto);
        return Ok(ApiResponse<SalaryStructureResponseDto>.SuccessResponse(
            result, "Salary structure assigned successfully."));
    }

    /// <summary>Get the current active salary structure for an employee.</summary>
    [HttpGet("employee/{employeeId:int}/current")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor}")]
    public async Task<IActionResult> GetCurrent(int employeeId)
    {
        var result = await _service.GetCurrentForEmployeeAsync(employeeId);
        return Ok(ApiResponse<SalaryStructureResponseDto>.SuccessResponse(result));
    }

    /// <summary>Get salary structure revision history for an employee.</summary>
    [HttpGet("employee/{employeeId:int}/history")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> GetHistory(int employeeId)
    {
        var result = await _service.GetHistoryForEmployeeAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<SalaryStructureResponseDto>>.SuccessResponse(result));
    }

    /// <summary>Get my own salary structure (employee self-service).</summary>
    [HttpGet("my-salary")]
    [Authorize(Roles = AppConstants.Roles.Employee)]
    public async Task<IActionResult> GetMySalary()
    {
        var empId = _currentUser.EmployeeId
            ?? throw new Core.Exceptions.UnauthorizedException("Employee profile not found.");
        var result = await _service.GetCurrentForEmployeeAsync(empId);
        return Ok(ApiResponse<SalaryStructureResponseDto>.SuccessResponse(result));
    }
}
