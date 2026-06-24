using BCrypt.Net;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs.Auth;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using EasyPay.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasyPay.Tests.Auth;

[TestFixture]
public class AuthServiceNUnitTests
{
    private Mock<IUserRepository>      _userRepoMock;
    private Mock<IJwtService>          _jwtMock;
    private Mock<IAuditService>        _auditMock;
    private Mock<IEmailService> _emailMock;
    private Mock<IConfiguration>       _configMock;
    private Mock<ILogger<AuthService>> _loggerMock;
    private AuthService                _sut;

    [SetUp]
    public void SetUp()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock      = new Mock<IJwtService>();
        _auditMock    = new Mock<IAuditService>();
        _emailMock = new Mock<IEmailService>();
        _configMock   = new Mock<IConfiguration>();
        _loggerMock   = new Mock<ILogger<AuthService>>();

        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(s => s["AccessTokenMinutes"]).Returns("60");
        jwtSection.Setup(s => s["RefreshTokenDays"]).Returns("7");
        _configMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);

        _sut = new AuthService(
            _userRepoMock.Object,
            _jwtMock.Object,
            _auditMock.Object,
            _emailMock.Object, 
            _configMock.Object,
            _loggerMock.Object);
    }


    [Test]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync("existing@test.com")).ReturnsAsync(true);

        var dto = new RegisterRequestDto
        {
            Username = "testuser", Email = "existing@test.com",
            Password = "Password@123", ConfirmPassword = "Password@123", RoleId = 4
        };

        Func<Task> act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(AppConstants.ErrorMessages.DuplicateEmail);
    }

    [Test]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ThrowsConflictException()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.UsernameExistsAsync("existinguser")).ReturnsAsync(true);

        var dto = new RegisterRequestDto
        {
            Username = "existinguser", Email = "new@test.com",
            Password = "Password@123", ConfirmPassword = "Password@123", RoleId = 4
        };

        Func<Task> act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(AppConstants.ErrorMessages.DuplicateUsername);
    }

    [Test]
    public async Task RegisterAsync_WithValidData_CreatesUserAndReturnsToken()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.UserId = 1; return u; });

        var savedUser = new User
        {
            UserId = 1, Username = "newuser", Email = "new@test.com",
            Role = new Role { RoleId = 4, RoleName = "Employee" }, RoleId = 4
        };

        _userRepoMock.Setup(r => r.GetWithRoleAsync(1)).ReturnsAsync(savedUser);
        _jwtMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("mock-access-token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("mock-refresh-token");

        var dto = new RegisterRequestDto
        {
            Username = "newuser", Email = "new@test.com",
            Password = "Password@123", ConfirmPassword = "Password@123", RoleId = 4
        };

        var result = await _sut.RegisterAsync(dto);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock-access-token");
        result.RefreshToken.Should().Be("mock-refresh-token");
        result.User.Email.Should().Be("new@test.com");
        result.User.Role.Should().Be("Employee");
    }


    [Test]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var dto = new LoginRequestDto { Email = "ghost@test.com", Password = "any" };

        Func<Task> act = () => _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(AppConstants.ErrorMessages.InvalidCredentials);
    }

    [Test]
    public async Task LoginAsync_WithLockedAccount_ThrowsUnauthorizedException()
    {
        var lockedUser = new User
        {
            UserId = 1, Email = "locked@test.com", IsActive = true,
            LockoutEnd = DateTime.UtcNow.AddMinutes(20),
            Role = new Role { RoleName = "Employee" }
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("locked@test.com")).ReturnsAsync(lockedUser);

        var dto = new LoginRequestDto { Email = "locked@test.com", Password = "any" };

        Func<Task> act = () => _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(AppConstants.ErrorMessages.AccountLocked);
    }

    [Test]
    public async Task LoginAsync_WithInactiveAccount_ThrowsUnauthorizedException()
    {
        var inactiveUser = new User
        {
            UserId = 1, Email = "inactive@test.com", IsActive = false,
            Role = new Role { RoleName = "Employee" }
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("inactive@test.com")).ReturnsAsync(inactiveUser);

        var dto = new LoginRequestDto { Email = "inactive@test.com", Password = "any" };

        Func<Task> act = () => _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(AppConstants.ErrorMessages.AccountInactive);
    }

    [Test]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAndIncrementsFailedAttempts()
    {
        var user = new User
        {
            UserId = 1, Email = "user@test.com", IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword@123"),
            FailedLoginAttempts = 0, Role = new Role { RoleName = "Employee" }
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("user@test.com")).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.IncrementFailedAttemptsAsync(1)).Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(),
            It.IsAny<bool>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var dto = new LoginRequestDto { Email = "user@test.com", Password = "WrongPassword@123" };

        Func<Task> act = () => _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _userRepoMock.Verify(r => r.IncrementFailedAttemptsAsync(1), Times.Once);
    }

    [Test]
    public async Task LoginAsync_WithCorrectCredentials_ReturnsAuthResponse()
    {
        var role = new Role { RoleId = 4, RoleName = "Employee" };
        var user = new User
        {
            UserId = 1, Email = "user@test.com", IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct@123"),
            FailedLoginAttempts = 0, Role = role, RoleId = 4
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("user@test.com")).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.ResetFailedAttemptsAsync(1)).Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.UpdateRefreshTokenAsync(1, It.IsAny<string>(), It.IsAny<DateTime?>()))
                     .Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.UpdateLastLoginAsync(1)).Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.GetWithEmployeeAsync(1)).ReturnsAsync(user);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, null, true, null)).Returns(Task.CompletedTask);
        _jwtMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");

        var dto = new LoginRequestDto { Email = "user@test.com", Password = "Correct@123" };

        var result = await _sut.LoginAsync(dto);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.User.UserId.Should().Be(1);
    }


    [Test]
    public async Task RefreshTokenAsync_WithInvalidAccessToken_ThrowsUnauthorizedException()
    {
        _jwtMock.Setup(j => j.GetPrincipalFromExpiredToken(It.IsAny<string>()))
                .Returns((System.Security.Claims.ClaimsPrincipal?)null);

        var dto = new RefreshTokenRequestDto
        {
            AccessToken = "invalid-token", RefreshToken = "some-refresh-token"
        };

        Func<Task> act = () => _sut.RefreshTokenAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(AppConstants.ErrorMessages.TokenExpired);
    }

    [Test]
    public async Task RefreshTokenAsync_WithExpiredRefreshToken_ThrowsUnauthorizedException()
    {
        var user = new User
        {
            UserId = 1, RefreshToken = "valid-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1),
            Role = new Role { RoleName = "Employee" }
        };

        _jwtMock.Setup(j => j.GetPrincipalFromExpiredToken(It.IsAny<string>()))
                .Returns(new System.Security.Claims.ClaimsPrincipal());
        _userRepoMock.Setup(r => r.GetByRefreshTokenAsync("valid-token")).ReturnsAsync(user);

        var dto = new RefreshTokenRequestDto
        {
            AccessToken = "expired-access", RefreshToken = "valid-token"
        };

        Func<Task> act = () => _sut.RefreshTokenAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }


    [Test]
    public async Task ChangePasswordAsync_WithWrongCurrentPassword_ThrowsUnauthorizedException()
    {
        var user = new User
        {
            UserId = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword@123")
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var dto = new ChangePasswordRequestDto
        {
            CurrentPassword = "WrongPassword@123",
            NewPassword = "NewPassword@123", ConfirmNewPassword = "NewPassword@123"
        };

        Func<Task> act = () => _sut.ChangePasswordAsync(1, dto);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Current password is incorrect.");
    }

    [Test]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_UpdatesPassword()
    {
        var user = new User
        {
            UserId = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword@123")
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdatePasswordAsync(1, It.IsAny<string>())).Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.UpdateRefreshTokenAsync(1, null, null)).Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, null, true, null)).Returns(Task.CompletedTask);

        var dto = new ChangePasswordRequestDto
        {
            CurrentPassword = "OldPassword@123",
            NewPassword = "NewPassword@123", ConfirmNewPassword = "NewPassword@123"
        };

        await _sut.ChangePasswordAsync(1, dto);

        _userRepoMock.Verify(r => r.UpdatePasswordAsync(1, It.IsAny<string>()), Times.Once);
        _userRepoMock.Verify(r => r.UpdateRefreshTokenAsync(1, null, null), Times.Once);
    }


    [Test]
    public async Task LogoutAsync_WithValidUser_InvalidatesRefreshToken()
    {
        var user = new User { UserId = 1, Email = "user@test.com" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateRefreshTokenAsync(1, null, null)).Returns(Task.CompletedTask);
        _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null, null, true, null)).Returns(Task.CompletedTask);

        await _sut.LogoutAsync(1, "some-refresh-token");

        _userRepoMock.Verify(r => r.UpdateRefreshTokenAsync(1, null, null), Times.Once);
    }
}
