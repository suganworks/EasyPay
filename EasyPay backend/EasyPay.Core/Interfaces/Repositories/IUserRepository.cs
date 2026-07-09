using EasyPay.Core.Entities;

namespace EasyPay.Core.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task<User?> GetWithRoleAsync(int userId);
    Task<User?> GetWithEmployeeAsync(int userId);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task UpdateRefreshTokenAsync(int userId, string? refreshToken, DateTime? expiry);
    Task UpdateResetTokenAsync(int userId, string token, DateTime expiry);
    Task UpdatePasswordAsync(int userId, string passwordHash);
    Task UpdateLastLoginAsync(int userId);
    Task IncrementFailedAttemptsAsync(int userId);
    Task ResetFailedAttemptsAsync(int userId);
    Task SetLockoutAsync(int userId, DateTime? lockoutEnd);
}
