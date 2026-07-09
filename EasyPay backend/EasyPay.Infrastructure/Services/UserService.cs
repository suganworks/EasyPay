using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Auth;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EasyPay.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IAuditService auditService,
        ICurrentUserService currentUser,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _auditService = auditService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PagedResponse<UserInfoDto>> GetPagedUsersAsync(PaginationParams pagination, string? role = null, bool? isActive = null)
    {
        var allUsers = await _userRepository.GetAllAsync();
        
        if (isActive.HasValue)
            allUsers = allUsers.Where(u => u.IsActive == isActive.Value).ToList();

        var paged = allUsers.Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize).ToList();
        
        var dtos = paged.Select(u => new UserInfoDto 
        { 
            UserId = u.UserId, 
            Username = u.Username, 
            Email = u.Email, 
            Role = u.RoleId.ToString() 
        }).ToList();

        return new PagedResponse<UserInfoDto> { Data = dtos, TotalCount = allUsers.Count(), PageNumber = pagination.PageNumber, PageSize = pagination.PageSize };
    }

    public async Task<UserInfoDto> GetByIdAsync(int userId)
    {
        var user = await _userRepository.GetWithRoleAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        return new UserInfoDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role?.RoleName ?? "Unknown"
        };
    }

    public async Task UpdateRoleAsync(int userId, int roleId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        user.RoleId = roleId;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        
        await _auditService.LogAsync("UpdateRole", "User", userId.ToString(), newValues: new { RoleId = roleId });
    }

    public async Task ToggleStatusAsync(int userId, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        user.IsActive = isActive;
        if (!isActive)
        {
            user.LockoutEnd = DateTime.UtcNow.AddYears(100); // effectively locked
            user.RefreshToken = null;
        }
        else
        {
            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        await _auditService.LogAsync(isActive ? "Activate" : "Deactivate", "User", userId.ToString());
    }
}
