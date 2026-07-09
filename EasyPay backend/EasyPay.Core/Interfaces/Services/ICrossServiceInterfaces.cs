using EasyPay.Core.DTOs;
using EasyPay.Core.Entities;

namespace EasyPay.Core.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(string action, string? entityName = null, string? entityId = null,
        object? oldValues = null, object? newValues = null,
        bool isSuccess = true, string? errorMessage = null);

    Task<PagedResponse<AuditLog>> GetLogsAsync(PaginationParams pagination,
        int? userId = null, string? action = null, string? entityName = null,
        DateTime? fromDate = null, DateTime? toDate = null);
}

public interface INotificationService
{
    Task SendAsync(int userId, string title, string message,
        string type = "Info", string? referenceType = null, int? referenceId = null);

    Task SendBulkAsync(IEnumerable<int> userIds, string title, string message,
        string type = "Info", string? referenceType = null, int? referenceId = null);

    Task<PagedResponse<Notification>> GetForUserAsync(int userId, PaginationParams pagination);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    int? EmployeeId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
