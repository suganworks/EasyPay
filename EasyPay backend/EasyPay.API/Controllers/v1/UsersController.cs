using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Auth;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

/// <summary>
/// Handles user account management for Admins.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserInfoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationParams pagination, [FromQuery] string? role, [FromQuery] bool? isActive)
    {
        var result = await _userService.GetPagedUsersAsync(pagination, role, isActive);
        return Ok(ApiResponse<PagedResponse<UserInfoDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(ApiResponse<UserInfoDto>.SuccessResponse(result));
    }

    [HttpPatch("{id}/role")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] int roleId)
    {
        await _userService.UpdateRoleAsync(id, roleId);
        return Ok(ApiResponse.SuccessResponse("User role updated successfully."));
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleStatus(int id, [FromBody] bool isActive)
    {
        await _userService.ToggleStatusAsync(id, isActive);
        return Ok(ApiResponse.SuccessResponse($"User status changed to {(isActive ? "Active" : "Inactive")}."));
    }
}
