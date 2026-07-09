using EasyPay.Core.Constants;
using EasyPay.Core.DTOs.Auth;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace EasyPay.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IAuditService auditService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService     = jwtService;
        _auditService   = auditService;
        _emailService   = emailService;
        _configuration  = configuration;
        _logger         = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Validate uniqueness
        if (await _userRepository.EmailExistsAsync(request.Email.ToLower()))
            throw new ConflictException(AppConstants.ErrorMessages.DuplicateEmail);

        if (await _userRepository.UsernameExistsAsync(request.Username.ToLower()))
            throw new ConflictException(AppConstants.ErrorMessages.DuplicateUsername);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshDays  = int.TryParse(_configuration["JwtSettings:RefreshTokenDays"], out var d) ? d : 7;

        var user = new User
        {
            Username            = request.Username.ToLower(),
            Email               = request.Email.ToLower(),
            PasswordHash        = passwordHash,
            RoleId              = request.RoleId,
            IsActive            = true,
            RefreshToken        = refreshToken,
            RefreshTokenExpiry  = DateTime.UtcNow.AddDays(refreshDays)
        };

        await _userRepository.AddAsync(user);

        // Reload with role
        var savedUser = await _userRepository.GetWithRoleAsync(user.UserId)
            ?? throw new EasyPayException("User registration failed.");

        var accessToken = _jwtService.GenerateAccessToken(savedUser);
        var expiryMins  = int.TryParse(_configuration["JwtSettings:AccessTokenMinutes"], out var m) ? m : 60;

        await _auditService.LogAsync("Register", "User", user.UserId.ToString(),
            newValues: new { user.Username, user.Email, RoleId = user.RoleId });

        _logger.LogInformation("New user registered: {Email}", user.Email);

        return BuildAuthResponse(savedUser, accessToken, refreshToken,
            DateTime.UtcNow.AddMinutes(expiryMins));
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLower());

        if (user == null)
            throw new UnauthorizedException(AppConstants.ErrorMessages.InvalidCredentials);

        // Check lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedException(AppConstants.ErrorMessages.AccountLocked);

        if (!user.IsActive)
            throw new UnauthorizedException(AppConstants.ErrorMessages.AccountInactive);

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await _userRepository.IncrementFailedAttemptsAsync(user.UserId);

            var maxAttempts = AppConstants.Security.MaxFailedAttempts;
            if (user.FailedLoginAttempts + 1 >= maxAttempts)
            {
                var lockoutEnd = DateTime.UtcNow.AddMinutes(AppConstants.Security.LockoutMinutes);
                await _userRepository.SetLockoutAsync(user.UserId, lockoutEnd);
                _logger.LogWarning("User {Email} locked out after {Attempts} failed attempts", user.Email, maxAttempts);
            }

            await _auditService.LogAsync("LoginFailed", "User", user.UserId.ToString(),
                isSuccess: false, errorMessage: "Invalid credentials");

            throw new UnauthorizedException(AppConstants.ErrorMessages.InvalidCredentials);
        }

        // Reset failed attempts
        await _userRepository.ResetFailedAttemptsAsync(user.UserId);

        // Generate tokens
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshDays  = int.TryParse(_configuration["JwtSettings:RefreshTokenDays"], out var d) ? d : 7;

        await _userRepository.UpdateRefreshTokenAsync(user.UserId, refreshToken,
            DateTime.UtcNow.AddDays(refreshDays));
        await _userRepository.UpdateLastLoginAsync(user.UserId);

        // Reload user with employee info
        var fullUser    = await _userRepository.GetWithEmployeeAsync(user.UserId) ?? user;
        var accessToken = _jwtService.GenerateAccessToken(fullUser);
        var expiryMins  = int.TryParse(_configuration["JwtSettings:AccessTokenMinutes"], out var m) ? m : 60;

        await _auditService.LogAsync("Login", "User", user.UserId.ToString());
        _logger.LogInformation("User logged in: {Email}", user.Email);

        return BuildAuthResponse(fullUser, accessToken, refreshToken,
            DateTime.UtcNow.AddMinutes(expiryMins));
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            throw new UnauthorizedException(AppConstants.ErrorMessages.TokenExpired);

        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

        if (user == null || user.RefreshToken != request.RefreshToken
            || user.RefreshTokenExpiry <= DateTime.UtcNow)
            throw new UnauthorizedException(AppConstants.ErrorMessages.InvalidRefreshToken);

        var newAccessToken  = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshDays     = int.TryParse(_configuration["JwtSettings:RefreshTokenDays"], out var d) ? d : 7;

        await _userRepository.UpdateRefreshTokenAsync(user.UserId, newRefreshToken,
            DateTime.UtcNow.AddDays(refreshDays));

        var expiryMins = int.TryParse(_configuration["JwtSettings:AccessTokenMinutes"], out var m) ? m : 60;

        return BuildAuthResponse(user, newAccessToken, newRefreshToken,
            DateTime.UtcNow.AddMinutes(expiryMins));
    }

    public async Task LogoutAsync(int userId, string refreshToken)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return;

        await _userRepository.UpdateRefreshTokenAsync(userId, null, null);
        await _auditService.LogAsync("Logout", "User", userId.ToString());
        _logger.LogInformation("User logged out: UserId={UserId}", userId);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLower());
        if (user == null)
        {
            // Don't reveal whether email exists — silently succeed
            _logger.LogWarning("Password reset requested for unknown email: {Email}", request.Email);
            return;
        }

        var token   = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); // already uppercase
        var expiry  = DateTime.UtcNow.AddMinutes(30);

        // Use targeted update to reliably save the token (generic UpdateAsync has EF tracking issues)
        await _userRepository.UpdateResetTokenAsync(user.UserId, token, expiry);

        // Send actual reset email (Link)
        var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
        var resetLink   = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={token}";
        await _emailService.SendPasswordResetEmailAsync(user.Email, token, resetLink);

        await _auditService.LogAsync("ForgotPassword", "User", user.UserId.ToString());
        _logger.LogInformation("Password reset token generated for: {Email}", user.Email);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        _logger.LogInformation("ResetPassword attempt for email: {Email}, token length: {TokenLen}", 
            request.Email, request.Token?.Length ?? 0);

        // Case-insensitive token lookup
        var tokenUpper = request.Token?.ToUpperInvariant() ?? string.Empty;
        var user = await _userRepository.GetByPasswordResetTokenAsync(tokenUpper);

        _logger.LogInformation("Token lookup result: user found = {Found}", user != null);

        if (user == null)
        {
            _logger.LogWarning("No user found for reset token.");
            throw new UnauthorizedException(AppConstants.ErrorMessages.PasswordResetInvalid);
        }

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Email mismatch: stored={Stored}, provided={Provided}", user.Email, request.Email);
            throw new UnauthorizedException(AppConstants.ErrorMessages.PasswordResetInvalid);
        }

        if (user.PasswordResetExpiry == null || user.PasswordResetExpiry <= DateTime.UtcNow)
        {
            _logger.LogWarning("Token expired: expiry={Expiry}, now={Now}", user.PasswordResetExpiry, DateTime.UtcNow);
            throw new UnauthorizedException(AppConstants.ErrorMessages.PasswordResetInvalid);
        }

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 11);
        await _userRepository.UpdatePasswordAsync(user.UserId, newHash);

        await _auditService.LogAsync("PasswordReset", "User", user.UserId.ToString());
        _logger.LogInformation("Password reset completed for: {Email}", user.Email);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect.");

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 11);
        await _userRepository.UpdatePasswordAsync(userId, newHash);

        // Invalidate sessions
        await _userRepository.UpdateRefreshTokenAsync(userId, null, null);

        await _auditService.LogAsync("PasswordChange", "User", userId.ToString());
        _logger.LogInformation("Password changed for UserId: {UserId}", userId);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static AuthResponseDto BuildAuthResponse(User user, string accessToken,
        string refreshToken, DateTime expiresAt)
        => new()
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt    = expiresAt,
            User = new UserInfoDto
            {
                UserId     = user.UserId,
                Username   = user.Username,
                Email      = user.Email,
                Role       = user.Role?.RoleName ?? string.Empty,
                EmployeeId = user.Employee?.EmployeeId,
                FullName   = user.Employee?.FullName
            }
        };
}
