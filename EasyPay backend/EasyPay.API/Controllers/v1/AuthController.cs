using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Auth;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

/// <summary>
/// Handles all authentication operations: register, login, refresh, logout, password management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _authService        = authService;
        _currentUserService = currentUserService;
        _logger             = logger;
    }

    /// <summary>Register a new user.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(
                AppConstants.ErrorMessages.ValidationFailed,
                ModelState.ToDictionary(k => k.Key,
                    v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray())));

        var result = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register),
            ApiResponse<AuthResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.Created));
    }

    /// <summary>Login with email and password.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, AppConstants.SuccessMessages.LoggedIn));
    }

    /// <summary>Refresh an expired access token using a valid refresh token.</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var result = await _authService.RefreshTokenAsync(request);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result));
    }

    /// <summary>Logout the current user and invalidate their refresh token.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        var userId = _currentUserService.UserId;
        if (userId.HasValue)
            await _authService.LogoutAsync(userId.Value, request.RefreshToken);

        return Ok(ApiResponse.SuccessResponse(AppConstants.SuccessMessages.LoggedOut));
    }

    /// <summary>Request a password reset email.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        await _authService.ForgotPasswordAsync(request);
        // Always return 200 to avoid email enumeration
        return Ok(ApiResponse.SuccessResponse(
            "If an account with this email exists, a password reset link has been sent."));
    }

    /// <summary>Reset password using the token received via email.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse.SuccessResponse("Password has been reset successfully. Please log in."));
    }

    /// <summary>Change password for the currently authenticated user.</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse(AppConstants.ErrorMessages.ValidationFailed));

        var userId = _currentUserService.UserId
            ?? throw new Core.Exceptions.UnauthorizedException();

        await _authService.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse.SuccessResponse("Password changed successfully."));
    }

    /// <summary>Get the current authenticated user's profile.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var userInfo = new UserInfoDto
        {
            UserId     = _currentUserService.UserId ?? 0,
            Email      = _currentUserService.Email ?? string.Empty,
            Role       = _currentUserService.Role ?? string.Empty,
            EmployeeId = _currentUserService.EmployeeId
        };

        return Ok(ApiResponse<UserInfoDto>.SuccessResponse(userInfo));
    }
}
