using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Auth;

namespace EasyPay.Core.Interfaces.Services;

public interface IUserService
{
    Task<PagedResponse<UserInfoDto>> GetPagedUsersAsync(PaginationParams pagination, string? role = null, bool? isActive = null);
    Task<UserInfoDto> GetByIdAsync(int userId);
    Task UpdateRoleAsync(int userId, int roleId);
    Task ToggleStatusAsync(int userId, bool isActive);
}
