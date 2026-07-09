using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Benefit;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

// ─── BenefitsController ────────────────────────────────────────────────────────

/// <summary>Manage company benefits and assign them to employees.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class BenefitsController : ControllerBase
{
    private readonly IBenefitService _service;
    private readonly ICurrentUserService _currentUser;

    public BenefitsController(IBenefitService service, ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    /// <summary>Get all benefits.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<BenefitResponseDto>>.SuccessResponse(result));
    }

    /// <summary>Get benefit by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<BenefitResponseDto>.SuccessResponse(result));
    }

    /// <summary>Create a new benefit type.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    [ProducesResponseType(typeof(ApiResponse<BenefitResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBenefitDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.BenefitId },
            ApiResponse<BenefitResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.Created));
    }

    /// <summary>Update a benefit.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateBenefitDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<BenefitResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.Updated));
    }

    /// <summary>Assign a benefit to an employee.</summary>
    [HttpPost("assign/{employeeId:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Assign(int employeeId, [FromBody] AssignBenefitDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var result = await _service.AssignToEmployeeAsync(employeeId, dto);
        return Ok(ApiResponse<EmployeeBenefitResponseDto>.SuccessResponse(
            result, "Benefit assigned to employee successfully."));
    }

    /// <summary>Get all benefits for a specific employee.</summary>
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetForEmployee(int employeeId)
    {
        // Employee can only see their own benefits
        if (_currentUser.IsInRole(AppConstants.Roles.Employee) &&
            _currentUser.EmployeeId != employeeId)
            return Forbid();

        var result = await _service.GetForEmployeeAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<EmployeeBenefitResponseDto>>.SuccessResponse(result));
    }

    /// <summary>Get my own benefits (employee self-service).</summary>
    [HttpGet("my-benefits")]
    public async Task<IActionResult> GetMyBenefits()
    {
        var empId = _currentUser.EmployeeId
            ?? throw new UnauthorizedException(AppConstants.ErrorMessages.EmployeeProfileNotFound);
        var result = await _service.GetForEmployeeAsync(empId);
        return Ok(ApiResponse<IEnumerable<EmployeeBenefitResponseDto>>.SuccessResponse(result));
    }

    /// <summary>Remove a benefit from an employee.</summary>
    [HttpDelete("employee-benefit/{employeeBenefitId:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Remove(int employeeBenefitId)
    {
        await _service.RemoveFromEmployeeAsync(employeeBenefitId);
        return Ok(ApiResponse.SuccessResponse("Benefit removed from employee."));
    }
}
