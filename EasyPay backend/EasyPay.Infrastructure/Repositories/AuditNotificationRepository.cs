using EasyPay.Core.DTOs;
using EasyPay.Core.Entities;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPay.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly EasyPayDbContext _context;

    public AuditLogRepository(EasyPayDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAsync(
        PaginationParams pagination, int? userId = null, string? action = null,
        string? entityName = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action.Contains(action));
        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);
        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= fromDate);
        if (toDate.HasValue)
            query = query.Where(a => a.CreatedAt <= toDate);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return (items, total);
    }
}

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(EasyPayDbContext context) : base(context) { }

    public async Task<(IEnumerable<Notification> Items, int Total)> GetPagedForUserAsync(
        int userId, PaginationParams pagination)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .AsQueryable();

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(n => n
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, DateTime.UtcNow));
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _context.Notifications
            .Where(n => n.NotificationId == notificationId)
            .ExecuteUpdateAsync(n => n
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, DateTime.UtcNow));
    }
}
