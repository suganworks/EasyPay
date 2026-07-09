using EasyPay.Core.DTOs;
using EasyPay.Core.Entities;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace EasyPay.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository  = auditLogRepository;
        _currentUserService  = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string? entityName = null, string? entityId = null,
        object? oldValues = null, object? newValues = null,
        bool isSuccess = true, string? errorMessage = null)
    {
        var request = _httpContextAccessor.HttpContext?.Request;

        var log = new AuditLog
        {
            UserId       = _currentUserService.UserId,
            UserEmail    = _currentUserService.Email,
            Action       = action,
            EntityName   = entityName,
            EntityId     = entityId,
            OldValues    = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues    = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IPAddress    = request?.HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent    = request?.Headers["User-Agent"].ToString(),
            IsSuccess    = isSuccess,
            ErrorMessage = errorMessage,
            CreatedAt    = DateTime.UtcNow
        };

        await _auditLogRepository.LogAsync(log);
    }

    public async Task<PagedResponse<AuditLog>> GetLogsAsync(PaginationParams pagination,
        int? userId = null, string? action = null, string? entityName = null,
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var (items, total) = await _auditLogRepository.GetPagedAsync(
            pagination, userId, action, entityName, fromDate, toDate);

        return new PagedResponse<AuditLog>
        {
            Data        = items,
            TotalCount  = total,
            PageNumber  = pagination.PageNumber,
            PageSize    = pagination.PageSize
        };
    }
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task SendAsync(int userId, string title, string message,
        string type = "Info", string? referenceType = null, int? referenceId = null)
    {
        var notification = new Notification
        {
            UserId           = userId,
            Title            = title,
            Message          = message,
            NotificationType = type,
            ReferenceType    = referenceType,
            ReferenceId      = referenceId
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task SendBulkAsync(IEnumerable<int> userIds, string title, string message,
        string type = "Info", string? referenceType = null, int? referenceId = null)
    {
        var notifications = userIds.Select(uid => new Notification
        {
            UserId           = uid,
            Title            = title,
            Message          = message,
            NotificationType = type,
            ReferenceType    = referenceType,
            ReferenceId      = referenceId
        });

        await _notificationRepository.AddRangeAsync(notifications);
    }

    public async Task<PagedResponse<Notification>> GetForUserAsync(int userId, PaginationParams pagination)
    {
        var (items, total) = await _notificationRepository.GetPagedForUserAsync(userId, pagination);
        return new PagedResponse<Notification>
        {
            Data       = items,
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize   = pagination.PageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _notificationRepository.GetUnreadCountAsync(userId);

    public async Task MarkAsReadAsync(int notificationId, int userId)
        => await _notificationRepository.MarkAsReadAsync(notificationId);

    public async Task MarkAllAsReadAsync(int userId)
        => await _notificationRepository.MarkAllAsReadAsync(userId);
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var value = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;
    public string? Role  => User?.FindFirst(ClaimTypes.Role)?.Value;

    public int? EmployeeId
    {
        get
        {
            var value = User?.FindFirst("EmployeeId")?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) => User?.IsInRole(role) == true;
}
