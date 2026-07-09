using Asp.Versioning;
using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyPay.API.Controllers.v1;

// ─── NotificationsController ──────────────────────────────────────────────────

/// <summary>Retrieve and manage user notifications.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ICurrentUserService  _currentUser;

    public NotificationsController(
        INotificationService service,
        ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    /// <summary>Get all notifications for the logged-in user (paged).</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] PaginationParams pagination)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var result = await _service.GetForUserAsync(userId, pagination);
        return Ok(result);
    }

    /// <summary>Get the unread notification count for the logged-in user.</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var count = await _service.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<object>.SuccessResponse(new { unreadCount = count }));
    }

    /// <summary>Mark a single notification as read.</summary>
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        await _service.MarkAsReadAsync(id, userId);
        return Ok(ApiResponse.SuccessResponse("Notification marked as read."));
    }

    /// <summary>Mark all notifications as read for the logged-in user.</summary>
    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        await _service.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse.SuccessResponse("All notifications marked as read."));
    }
}

// ─── AuditLogsController ──────────────────────────────────────────────────────

/// <summary>View system audit trail — Admin only.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit-logs")]
[Authorize(Roles = AppConstants.Roles.Admin)]
[Produces("application/json")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Get paged audit log entries with optional filters.
    /// Supports filtering by userId, action, entityName, and date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AuditLog>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] PaginationParams pagination,
        [FromQuery] int?      userId     = null,
        [FromQuery] string?   action     = null,
        [FromQuery] string?   entityName = null,
        [FromQuery] DateTime? fromDate   = null,
        [FromQuery] DateTime? toDate     = null)
    {
        var result = await _auditService.GetLogsAsync(
            pagination, userId, action, entityName, fromDate, toDate);
        return Ok(result);
    }
}
