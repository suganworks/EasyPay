using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Employee;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

/// <summary>Manage employees — create, update, search, activate/deactivate.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;
    private readonly ICurrentUserService _currentUser;

    public EmployeesController(IEmployeeService service, ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    /// <summary>Get a paged list of all employees.</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination,
                                             [FromQuery] int? departmentId = null,
                                             [FromQuery] int? designationId = null)
    {
        var result = await _service.GetPagedAsync(pagination, departmentId, designationId);
        return Ok(result);
    }

    /// <summary>Get employee by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Employees can only view their own record
        if (_currentUser.IsInRole(AppConstants.Roles.Employee))
        {
            var empId = _currentUser.EmployeeId;
            if (empId != id)
                return Forbid();
        }
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<EmployeeResponseDto>.SuccessResponse(result));
    }

    /// <summary>Get employee by employee code.</summary>
    [HttpGet("code/{code}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.PayrollProcessor}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _service.GetByCodeAsync(code);
        return Ok(ApiResponse<EmployeeResponseDto>.SuccessResponse(result));
    }

    /// <summary>Get employees by department.</summary>
    [HttpGet("by-department/{departmentId:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var result = await _service.GetByDepartmentAsync(departmentId);
        return Ok(ApiResponse<IEnumerable<EmployeeListDto>>.SuccessResponse(result));
    }

    /// <summary>Get direct reports for a manager.</summary>
    [HttpGet("my-team")]
    [Authorize(Roles = $"{AppConstants.Roles.Manager},{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> GetMyTeam()
    {
        var managerId = _currentUser.EmployeeId
            ?? throw new Core.Exceptions.UnauthorizedException("Manager employee profile not found.");
        var result = await _service.GetByManagerAsync(managerId);
        return Ok(ApiResponse<IEnumerable<EmployeeListDto>>.SuccessResponse(result));
    }

    /// <summary>Onboard a new employee (creates user account + employee record).</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.EmployeeId },
            ApiResponse<EmployeeResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.Created));
    }

    /// <summary>Update employee information.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<EmployeeResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.Updated));
    }

    /// <summary>Deactivate / offboard an employee.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);
        return Ok(ApiResponse.SuccessResponse("Employee deactivated successfully."));
    }

    /// <summary>Reactivate a previously deactivated employee.</summary>
    [HttpPatch("{id:int}/reactivate")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.HRManager}")]
    public async Task<IActionResult> Reactivate(int id)
    {
        await _service.ReactivateAsync(id);
        return Ok(ApiResponse.SuccessResponse("Employee reactivated successfully."));
    }
}
