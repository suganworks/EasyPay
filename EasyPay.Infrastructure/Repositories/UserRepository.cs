using EasyPay.Core.Entities;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPay.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(EasyPayDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username.ToLower());

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        => await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

    public async Task<User?> GetByPasswordResetTokenAsync(string token)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token);

    public async Task<User?> GetWithRoleAsync(int userId)
        => await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<User?> GetWithEmployeeAsync(int userId)
        => await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Department)
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Designation)
            .FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email.ToLower());

    public async Task<bool> UsernameExistsAsync(string username)
        => await _context.Users.AnyAsync(u => u.Username == username.ToLower());

    public async Task UpdateRefreshTokenAsync(int userId, string? refreshToken, DateTime? expiry)
    {
        await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.RefreshToken, refreshToken)
                .SetProperty(x => x.RefreshTokenExpiry, expiry)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    }

    public async Task UpdatePasswordAsync(int userId, string passwordHash)
    {
        await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.PasswordHash, passwordHash)
                .SetProperty(x => x.PasswordResetToken, (string?)null)
                .SetProperty(x => x.PasswordResetExpiry, (DateTime?)null)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.LastLoginAt, DateTime.UtcNow)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    }

    public async Task IncrementFailedAttemptsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.FailedLoginAttempts++;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ResetFailedAttemptsAsync(int userId)
    {
        await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.FailedLoginAttempts, 0)
                .SetProperty(x => x.LockoutEnd, (DateTime?)null)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    }

    public async Task SetLockoutAsync(int userId, DateTime? lockoutEnd)
    {
        await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.LockoutEnd, lockoutEnd)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    }
}
