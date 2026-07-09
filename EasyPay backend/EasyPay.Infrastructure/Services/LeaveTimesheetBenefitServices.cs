using EasyPay.Core.Constants;
using EasyPay.Core.DTOs;
using EasyPay.Core.DTOs.Benefit;
using EasyPay.Core.DTOs.Leave;
using EasyPay.Core.DTOs.Timesheet;
using EasyPay.Core.Entities;
using EasyPay.Core.Exceptions;
using EasyPay.Core.Interfaces.Repositories;
using EasyPay.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EasyPay.Infrastructure.Services;
// ─── LeaveService ─────────────────────────────────────────────────────────────
public class LeaveService : ILeaveService
{
    private readonly ILeaveRequestRepository _leaveRepo;
    private readonly ILeaveTypeRepository _leaveTypeRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(
        ILeaveRequestRepository leaveRepo,
        ILeaveTypeRepository leaveTypeRepo,
        IEmployeeRepository employeeRepo,
        IAuditService auditService,
        INotificationService notificationService,
        ICurrentUserService currentUser,
        IUserRepository userRepo,
        IEmailService emailService,
        ILogger<LeaveService> logger)
    {
        _leaveRepo           = leaveRepo;
        _leaveTypeRepo       = leaveTypeRepo;
        _employeeRepo        = employeeRepo;
        _auditService        = auditService;
        _notificationService = notificationService;
        _currentUser         = currentUser;
        _userRepo            = userRepo;
        _emailService        = emailService;
        _logger              = logger;
    }

    public async Task<LeaveRequestResponseDto> SubmitAsync(int employeeId, CreateLeaveRequestDto dto)
    {
        var employee = await _employeeRepo.GetWithDetailsAsync(employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, employeeId);

        var leaveType = await _leaveTypeRepo.GetByIdAsync(dto.LeaveTypeId)
            ?? throw new NotFoundException("LeaveType", dto.LeaveTypeId);

        if (dto.ToDate < dto.FromDate)
            throw new ValidationException("ToDate", "End date must be on or after start date.");

        if (await _leaveRepo.HasOverlappingLeaveAsync(employeeId, dto.FromDate, dto.ToDate))
            throw new BusinessRuleException("You already have an approved or pending leave for this period.");

        var totalDays = dto.IsHalfDay ? 1 : (dto.ToDate.DayNumber - dto.FromDate.DayNumber + 1);

        if (leaveType.MaxDaysPerYear > 0)
        {
            var usedDays = await _leaveRepo.GetUsedLeaveDaysAsync(employeeId, dto.LeaveTypeId, dto.FromDate.Year);
            if (usedDays + totalDays > leaveType.MaxDaysPerYear)
                throw new BusinessRuleException(
                    $"Insufficient {leaveType.LeaveTypeName} balance. Available: {leaveType.MaxDaysPerYear - usedDays} days.");
        }

        var leave = new LeaveRequest
        {
            EmployeeId  = employeeId,
            LeaveTypeId = dto.LeaveTypeId,
            FromDate    = dto.FromDate,
            ToDate      = dto.ToDate,
            TotalDays   = totalDays,
            Reason      = dto.Reason,
            Status      = AppConstants.WorkflowStatus.Pending,
            IsHalfDay   = dto.IsHalfDay,
            HalfDayType = dto.HalfDayType
        };

        await _leaveRepo.AddAsync(leave);

        if (employee.ManagerId.HasValue)
        {
            var manager = await _employeeRepo.GetWithDetailsAsync(employee.ManagerId.Value);
            if (manager != null)
            {
                await _notificationService.SendAsync(
                    manager.UserId,
                    "New Leave Request",
                    $"{employee.FullName} has submitted a {leaveType.LeaveTypeName} request for {dto.FromDate:dd MMM} - {dto.ToDate:dd MMM yyyy}.",
                    "Info", "Leave", leave.LeaveRequestId);
                
                try
                {
                    var managerUser = await _userRepo.GetByIdAsync(manager.UserId);
                    if (managerUser != null)
                    {
                        var subject = $"New Leave Request: {employee.FullName}";
                        var body = $@"
                            <h3>New Leave Request</h3>
                            <p><strong>{employee.FullName}</strong> has submitted a new leave request.</p>
                            <ul>
                                <li><strong>Type:</strong> {leaveType.LeaveTypeName}</li>
                                <li><strong>From:</strong> {dto.FromDate:dd MMM yyyy}</li>
                                <li><strong>To:</strong> {dto.ToDate:dd MMM yyyy}</li>
                                <li><strong>Total Days:</strong> {totalDays}</li>
                                <li><strong>Reason:</strong> {dto.Reason ?? "N/A"}</li>
                            </ul>
                            <p>Please log in to the portal to approve or reject this request.</p>";
                        
                        await _emailService.SendEmailAsync(managerUser.Email, subject, body);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send leave request email to manager {ManagerId}", manager.UserId);
                }
            }
        }

        await _auditService.LogAsync("SubmitLeave", AppConstants.EntityNames.LeaveRequest, leave.LeaveRequestId.ToString(),
            newValues: new { leave.EmployeeId, leave.LeaveTypeId, leave.FromDate, leave.ToDate });

        _logger.LogInformation("Leave request submitted by Employee {Id} for {Type}", employeeId, leaveType.LeaveTypeName);

        return await GetByIdAsync(leave.LeaveRequestId);
    }

    public async Task<LeaveRequestResponseDto> ApproveOrRejectAsync(int leaveRequestId, int approverId, ApproveLeaveDto dto)
    {
        var leave = await _leaveRepo.FirstOrDefaultAsync(l => l.LeaveRequestId == leaveRequestId)
            ?? throw new NotFoundException(AppConstants.EntityNames.LeaveRequest, leaveRequestId);

        if (leave.Status != AppConstants.WorkflowStatus.Pending)
            throw new BusinessRuleException($"Leave request is already '{leave.Status}'.");

        var action = dto.Action.Trim();
        if (action != AppConstants.WorkflowStatus.Approved && action != AppConstants.WorkflowStatus.Rejected)
            throw new ValidationException("Action", "Action must be 'Approved' or 'Rejected'.");

        if (action == AppConstants.WorkflowStatus.Rejected && string.IsNullOrWhiteSpace(dto.RejectionReason))
            throw new ValidationException("RejectionReason", "Rejection reason is required.");

        leave.Status          = action;
        leave.ApprovedById    = approverId == 0 ? null : approverId;
        leave.ApprovedAt      = DateTime.UtcNow;
        leave.RejectionReason = dto.RejectionReason;
        leave.UpdatedAt       = DateTime.UtcNow;

        await _leaveRepo.UpdateAsync(leave);

        var employee = await _employeeRepo.GetWithDetailsAsync(leave.EmployeeId);
        if (employee != null)
        {
            var msg = action == AppConstants.WorkflowStatus.Approved
                ? $"Your leave request ({leave.FromDate:dd MMM} - {leave.ToDate:dd MMM yyyy}) has been approved."
                : $"Your leave request ({leave.FromDate:dd MMM} - {leave.ToDate:dd MMM yyyy}) was rejected. Reason: {dto.RejectionReason}";

            await _notificationService.SendAsync(
                employee.UserId, $"Leave {action}", msg,
                action == AppConstants.WorkflowStatus.Approved ? "Success" : "Warning",
                "Leave", leaveRequestId);

            try
            {
                var user = await _userRepo.GetByIdAsync(employee.UserId);
                if (user != null)
                {
                    await _emailService.SendLeaveStatusEmailAsync(
                        user.Email,
                        employee.FirstName,
                        leave.LeaveType?.LeaveTypeName ?? "Leave",
                        action,
                        dto.RejectionReason
                    );
                    _logger.LogInformation("Leave status email sent to {Email}", user.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send leave status email for LeaveRequestId {Id}", leaveRequestId);
            }
        }

        await _auditService.LogAsync($"Leave{action}", AppConstants.EntityNames.LeaveRequest, leaveRequestId.ToString(),
            newValues: new { leave.Status, leave.RejectionReason });

        return await GetByIdAsync(leaveRequestId);
    }

    public async Task<LeaveRequestResponseDto> CancelAsync(int leaveRequestId, int employeeId)
    {
        var leave = await _leaveRepo.FirstOrDefaultAsync(l =>
            l.LeaveRequestId == leaveRequestId && l.EmployeeId == employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.LeaveRequest, leaveRequestId);

        if (leave.Status == AppConstants.WorkflowStatus.Approved && leave.FromDate <= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleException("Cannot cancel a leave that has already started.");

        leave.Status    = AppConstants.WorkflowStatus.Cancelled;
        leave.UpdatedAt = DateTime.UtcNow;

        await _leaveRepo.UpdateAsync(leave);
        await _auditService.LogAsync("CancelLeave", AppConstants.EntityNames.LeaveRequest, leaveRequestId.ToString());

        return await GetByIdAsync(leaveRequestId);
    }

    public async Task<LeaveRequestResponseDto> GetByIdAsync(int leaveRequestId)
    {
        var items = await _leaveRepo.FindAsync(l => l.LeaveRequestId == leaveRequestId);
        var leave = items.FirstOrDefault()
            ?? throw new NotFoundException(AppConstants.EntityNames.LeaveRequest, leaveRequestId);
        return MapToResponse(leave);
    }

    public async Task<PagedResponse<LeaveRequestResponseDto>> GetPagedAsync(
        PaginationParams pagination, int? employeeId = null, string? status = null)
    {
        var (items, total) = await _leaveRepo.GetPagedAsync(pagination, employeeId, status);
        return new PagedResponse<LeaveRequestResponseDto>
        {
            Data       = items.Select(MapToResponse),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize   = pagination.PageSize
        };
    }

    public async Task<IEnumerable<LeaveRequestResponseDto>> GetPendingForManagerAsync(int managerId)
    {
        var items = await _leaveRepo.GetPendingForManagerAsync(managerId);
        return items.Select(MapToResponse);
    }

    public async Task<IEnumerable<LeaveBalanceDto>> GetBalancesAsync(int employeeId, int year)
    {
        var leaveTypes = await _leaveTypeRepo.GetActiveAsync();
        var balances   = new List<LeaveBalanceDto>();

        foreach (var lt in leaveTypes.Where(lt => lt.MaxDaysPerYear > 0))
        {
            var used = await _leaveRepo.GetUsedLeaveDaysAsync(employeeId, lt.LeaveTypeId, year);
            balances.Add(new LeaveBalanceDto
            {
                LeaveTypeId    = lt.LeaveTypeId,
                LeaveTypeName  = lt.LeaveTypeName,
                LeaveCode      = lt.LeaveCode,
                MaxDaysPerYear = lt.MaxDaysPerYear,
                UsedDays       = used,
                RemainingDays  = Math.Max(lt.MaxDaysPerYear - used, 0),
                IsPaid         = lt.IsPaid
            });
        }

        return balances;
    }

    public async Task<LeaveCarryForwardDto> ProcessCarryForwardAsync(int employeeId, int fromYear)
    {
        var employee = await _employeeRepo.GetWithDetailsAsync(employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, employeeId);

        var leaveTypes = await _leaveTypeRepo.GetActiveAsync();
        var items      = new List<LeaveCarryForwardItemDto>();

        foreach (var lt in leaveTypes.Where(lt => lt.IsCarryForward && lt.MaxDaysPerYear > 0))
        {
            var usedDays      = await _leaveRepo.GetUsedLeaveDaysAsync(employeeId, lt.LeaveTypeId, fromYear);
            var remainingDays = Math.Max(lt.MaxDaysPerYear - usedDays, 0);
            var carryDays     = Math.Min(remainingDays, lt.MaxCarryForward);
            var lapsedDays    = remainingDays - carryDays;

            items.Add(new LeaveCarryForwardItemDto
            {
                LeaveTypeId        = lt.LeaveTypeId,
                LeaveTypeName      = lt.LeaveTypeName,
                EligibleDays       = remainingDays,
                CarriedForwardDays = carryDays,
                LapsedDays         = lapsedDays
            });
        }

        await _auditService.LogAsync("LeaveCarryForward", AppConstants.EntityNames.Employee, employeeId.ToString(),
            newValues: new { employeeId, fromYear, ItemCount = items.Count });

        return new LeaveCarryForwardDto
        {
            EmployeeId   = employeeId,
            EmployeeName = employee.FullName,
            CarryForwardItems = items
        };
    }

    public async Task<IEnumerable<LeaveCarryForwardDto>> BulkCarryForwardAsync(int fromYear)
    {
        var employees = await _employeeRepo.FindAsync(e => e.IsActive && e.EmploymentStatus == "Active");
        var results   = new List<LeaveCarryForwardDto>();

        foreach (var emp in employees)
        {
            var result = await ProcessCarryForwardAsync(emp.EmployeeId, fromYear);
            results.Add(result);
        }

        return results;
    }

    private static LeaveRequestResponseDto MapToResponse(LeaveRequest l) => new()
    {
        LeaveRequestId  = l.LeaveRequestId,
        EmployeeId      = l.EmployeeId,
        EmployeeName    = l.Employee?.FullName ?? string.Empty,
        EmployeeCode    = l.Employee?.EmployeeCode ?? string.Empty,
        DepartmentName  = l.Employee?.Department?.DepartmentName ?? string.Empty,
        LeaveTypeId     = l.LeaveTypeId,
        LeaveTypeName   = l.LeaveType?.LeaveTypeName ?? string.Empty,
        LeaveCode       = l.LeaveType?.LeaveCode ?? string.Empty,
        FromDate        = l.FromDate,
        ToDate          = l.ToDate,
        TotalDays       = l.TotalDays,
        Reason          = l.Reason,
        Status          = l.Status,
        ApprovedByName  = l.ApprovedBy?.FullName,
        ApprovedAt      = l.ApprovedAt,
        RejectionReason = l.RejectionReason,
        IsHalfDay       = l.IsHalfDay,
        HalfDayType     = l.HalfDayType,
        CreatedAt       = l.CreatedAt
    };
}

// ─── TimesheetService ─────────────────────────────────────────────────────────
public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IAuditService _auditService;

    public TimesheetService(
        ITimesheetRepository timesheetRepo,
        IEmployeeRepository employeeRepo,
        IAuditService auditService)
    {
        _timesheetRepo = timesheetRepo;
        _employeeRepo  = employeeRepo;
        _auditService  = auditService;
    }

    public async Task<TimesheetResponseDto> CreateAsync(int employeeId, CreateTimesheetDto dto)
    {
        _ = await _employeeRepo.GetByIdAsync(employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, employeeId);

        if (await _timesheetRepo.GetForDateAsync(employeeId, dto.WorkDate) != null)
            throw new ConflictException($"Timesheet already exists for {dto.WorkDate:dd MMM yyyy}.");

        if (dto.WorkDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleException("Cannot submit timesheet for a future date.");

        var timesheet = new Timesheet
        {
            EmployeeId    = employeeId,
            WorkDate      = dto.WorkDate,
            CheckIn       = dto.CheckIn,
            CheckOut      = dto.CheckOut,
            HoursWorked   = dto.HoursWorked,
            OvertimeHours = dto.OvertimeHours,
            Notes         = dto.Notes,
            Status        = AppConstants.WorkflowStatus.Pending
        };

        await _timesheetRepo.AddAsync(timesheet);
        await _auditService.LogAsync("Create", AppConstants.EntityNames.Timesheet, timesheet.TimesheetId.ToString(), newValues: dto);
        return await GetByIdAsync(timesheet.TimesheetId);
    }

    public async Task<TimesheetResponseDto> UpdateAsync(int timesheetId, int employeeId, UpdateTimesheetDto dto)
    {
        var timesheet = await _timesheetRepo.GetByIdAsync(timesheetId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Timesheet, timesheetId);

        if (timesheet.EmployeeId != employeeId)
            throw new UnauthorizedException("You can only update your own timesheets.");

        if (timesheet.Status == AppConstants.WorkflowStatus.Approved)
            throw new BusinessRuleException("Cannot edit an approved timesheet.");

        timesheet.CheckIn       = dto.CheckIn;
        timesheet.CheckOut      = dto.CheckOut;
        timesheet.HoursWorked   = dto.HoursWorked;
        timesheet.OvertimeHours = dto.OvertimeHours;
        timesheet.Notes         = dto.Notes;
        timesheet.UpdatedAt     = DateTime.UtcNow;

        await _timesheetRepo.UpdateAsync(timesheet);
        await _auditService.LogAsync("Update", AppConstants.EntityNames.Timesheet, timesheetId.ToString(), newValues: dto);
        return await GetByIdAsync(timesheetId);
    }

    public async Task<TimesheetResponseDto> ApproveOrRejectAsync(int timesheetId, int approverId, ApproveTimesheetDto dto)
    {
        var timesheet = await _timesheetRepo.GetByIdAsync(timesheetId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Timesheet, timesheetId);

        if (timesheet.Status != AppConstants.WorkflowStatus.Pending)
            throw new BusinessRuleException($"Cannot action timesheet that is '{timesheet.Status}'.");

        var action = dto.Action.Trim();
        if (action != AppConstants.WorkflowStatus.Approved && action != AppConstants.WorkflowStatus.Rejected)
            throw new ValidationException("Action", "Action must be 'Approved' or 'Rejected'.");

        timesheet.Status       = action;
        timesheet.ApprovedById = approverId == 0 ? null : approverId;
        timesheet.ApprovedAt   = DateTime.UtcNow;
        timesheet.UpdatedAt    = DateTime.UtcNow;

        await _timesheetRepo.UpdateAsync(timesheet);
        await _auditService.LogAsync($"Timesheet{action}", AppConstants.EntityNames.Timesheet, timesheetId.ToString());
        return await GetByIdAsync(timesheetId);
    }

    public async Task<TimesheetResponseDto> GetByIdAsync(int timesheetId)
    {
        var timesheet = await _timesheetRepo.FirstOrDefaultAsync(t => t.TimesheetId == timesheetId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Timesheet, timesheetId);
        return MapToResponse(timesheet);
    }

    public async Task<PagedResponse<TimesheetResponseDto>> GetPagedAsync(
        PaginationParams pagination, int? employeeId = null,
        DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var (items, total) = await _timesheetRepo.GetPagedAsync(pagination, employeeId, fromDate, toDate);
        return new PagedResponse<TimesheetResponseDto>
        {
            Data       = items.Select(MapToResponse),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize   = pagination.PageSize
        };
    }

    public async Task<int> BulkApproveAsync(List<int> timesheetIds, int approverId)
    {
        var approved = 0;
        foreach (var id in timesheetIds)
        {
            var timesheet = await _timesheetRepo.GetByIdAsync(id);
            if (timesheet == null || timesheet.Status != AppConstants.WorkflowStatus.Pending) continue;

            timesheet.Status       = AppConstants.WorkflowStatus.Approved;
            timesheet.ApprovedById = approverId == 0 ? null : approverId;
            timesheet.ApprovedAt   = DateTime.UtcNow;
            timesheet.UpdatedAt    = DateTime.UtcNow;

            await _timesheetRepo.UpdateAsync(timesheet);
            approved++;
        }

        await _auditService.LogAsync("BulkApproveTimesheets", AppConstants.EntityNames.Timesheet,
            null, newValues: new { Count = approved, ApprovedBy = approverId });

        return approved;
    }

    public async Task<TimesheetMonthlySummaryDto> GetMonthlySummaryAsync(
        int employeeId, int year, int month)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, employeeId);

        var fromDate = new DateOnly(year, month, 1);
        var toDate   = fromDate.AddMonths(1).AddDays(-1);

        var allTs      = (await _timesheetRepo.GetPagedAsync(
            new EasyPay.Core.DTOs.PaginationParams { PageNumber = 1, PageSize = 1000 },
            employeeId, fromDate, toDate)).Items.ToList();

        return new TimesheetMonthlySummaryDto
        {
            EmployeeId        = employeeId,
            EmployeeName      = $"{employee.FirstName} {employee.LastName}",
            Year              = year,
            Month             = month,
            MonthName         = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).ToString("MMMM yyyy"),
            TotalWorkingDays  = toDate.Day,
            PresentDays       = allTs.Select(t => t.WorkDate).Distinct().Count(),
            TotalHoursWorked  = allTs.Sum(t => t.HoursWorked),
            TotalOvertimeHours= allTs.Sum(t => t.OvertimeHours),
            PendingTimesheets = allTs.Count(t => t.Status == AppConstants.WorkflowStatus.Pending),
            ApprovedTimesheets= allTs.Count(t => t.Status == AppConstants.WorkflowStatus.Approved)
        };
    }

    private static TimesheetResponseDto MapToResponse(Timesheet t) => new()
    {
        TimesheetId    = t.TimesheetId,
        EmployeeId     = t.EmployeeId,
        EmployeeName   = t.Employee?.FullName ?? string.Empty,
        EmployeeCode   = t.Employee?.EmployeeCode ?? string.Empty,
        WorkDate       = t.WorkDate,
        CheckIn        = t.CheckIn,
        CheckOut       = t.CheckOut,
        HoursWorked    = t.HoursWorked,
        OvertimeHours  = t.OvertimeHours,
        Status         = t.Status,
        ApprovedByName = t.ApprovedBy?.FullName,
        ApprovedAt     = t.ApprovedAt,
        Notes          = t.Notes,
        CreatedAt      = t.CreatedAt
    };
}

// ─── BenefitService ───────────────────────────────────────────────────────────
public class BenefitService : IBenefitService
{
    private readonly IBenefitRepository _benefitRepo;
    private readonly IEmployeeBenefitRepository _empBenefitRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IAuditService _auditService;

    public BenefitService(
        IBenefitRepository benefitRepo,
        IEmployeeBenefitRepository empBenefitRepo,
        IEmployeeRepository employeeRepo,
        IAuditService auditService)
    {
        _benefitRepo    = benefitRepo;
        _empBenefitRepo = empBenefitRepo;
        _employeeRepo   = employeeRepo;
        _auditService   = auditService;
    }

    public async Task<BenefitResponseDto> CreateAsync(CreateBenefitDto dto)
    {
        if (await _benefitRepo.BenefitCodeExistsAsync(dto.BenefitCode.ToUpper()))
            throw new ConflictException($"Benefit code '{dto.BenefitCode}' already exists.");

        var benefit = new Benefit
        {
            BenefitName  = dto.BenefitName,
            BenefitCode  = dto.BenefitCode.ToUpper(),
            BenefitType  = dto.BenefitType,
            Amount       = dto.Amount,
            IsPercentage = dto.IsPercentage,
            Description  = dto.Description,
            IsActive     = true
        };

        await _benefitRepo.AddAsync(benefit);
        await _auditService.LogAsync("Create", AppConstants.EntityNames.Benefit, benefit.BenefitId.ToString(), newValues: dto);
        return MapToResponse(benefit);
    }

    public async Task<BenefitResponseDto> UpdateAsync(int benefitId, CreateBenefitDto dto)
    {
        var benefit = await _benefitRepo.GetByIdAsync(benefitId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Benefit, benefitId);

        benefit.BenefitName  = dto.BenefitName;
        benefit.BenefitType  = dto.BenefitType;
        benefit.Amount       = dto.Amount;
        benefit.IsPercentage = dto.IsPercentage;
        benefit.Description  = dto.Description;
        benefit.UpdatedAt    = DateTime.UtcNow;

        await _benefitRepo.UpdateAsync(benefit);
        await _auditService.LogAsync("Update", AppConstants.EntityNames.Benefit, benefitId.ToString(), newValues: dto);
        return MapToResponse(benefit);
    }

    public async Task<BenefitResponseDto> GetByIdAsync(int benefitId)
    {
        var benefit = await _benefitRepo.GetByIdAsync(benefitId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Benefit, benefitId);
        return MapToResponse(benefit);
    }

    public async Task<IEnumerable<BenefitResponseDto>> GetAllAsync()
    {
        var items = await _benefitRepo.GetAllAsync();
        return items.Select(MapToResponse);
    }

    public async Task<EmployeeBenefitResponseDto> AssignToEmployeeAsync(int employeeId, AssignBenefitDto dto)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Employee, employeeId);

        var benefit = await _benefitRepo.GetByIdAsync(dto.BenefitId)
            ?? throw new NotFoundException(AppConstants.EntityNames.Benefit, dto.BenefitId);

        await _empBenefitRepo.DeactivatePreviousAsync(employeeId, dto.BenefitId);

        var empBenefit = new EmployeeBenefit
        {
            EmployeeId    = employeeId,
            BenefitId     = dto.BenefitId,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo   = dto.EffectiveTo,
            OverrideAmount= dto.OverrideAmount,
            IsActive      = true
        };

        await _empBenefitRepo.AddAsync(empBenefit);
        await _auditService.LogAsync("AssignBenefit", AppConstants.EntityNames.EmployeeBenefit,
            empBenefit.EmployeeBenefitId.ToString(), newValues: dto);

        return MapToEmpResponse(empBenefit, employee, benefit);
    }

    public async Task<IEnumerable<EmployeeBenefitResponseDto>> GetForEmployeeAsync(int employeeId)
    {
        var items = await _empBenefitRepo.GetActiveForEmployeeAsync(employeeId);
        return items.Select(eb => MapToEmpResponse(eb, eb.Employee, eb.Benefit));
    }

    public async Task RemoveFromEmployeeAsync(int employeeBenefitId)
    {
        var eb = await _empBenefitRepo.GetByIdAsync(employeeBenefitId)
            ?? throw new NotFoundException(AppConstants.EntityNames.EmployeeBenefit, employeeBenefitId);

        eb.IsActive    = false;
        eb.EffectiveTo = DateOnly.FromDateTime(DateTime.UtcNow);
        eb.UpdatedAt   = DateTime.UtcNow;

        await _empBenefitRepo.UpdateAsync(eb);
        await _auditService.LogAsync("RemoveBenefit", AppConstants.EntityNames.EmployeeBenefit, employeeBenefitId.ToString());
    }

    private static BenefitResponseDto MapToResponse(Benefit b) => new()
    {
        BenefitId    = b.BenefitId,
        BenefitName  = b.BenefitName,
        BenefitCode  = b.BenefitCode,
        BenefitType  = b.BenefitType,
        Amount       = b.Amount,
        IsPercentage = b.IsPercentage,
        Description  = b.Description,
        IsActive     = b.IsActive
    };

    private static EmployeeBenefitResponseDto MapToEmpResponse(
        EmployeeBenefit eb, EasyPay.Core.Entities.Employee? emp, Benefit? b) => new()
    {
        EmployeeBenefitId = eb.EmployeeBenefitId,
        EmployeeId        = eb.EmployeeId,
        EmployeeName      = emp?.FullName ?? string.Empty,
        BenefitId         = eb.BenefitId,
        BenefitName       = b?.BenefitName ?? string.Empty,
        BenefitType       = b?.BenefitType ?? string.Empty,
        EffectiveFrom     = eb.EffectiveFrom,
        EffectiveTo       = eb.EffectiveTo,
        OverrideAmount    = eb.OverrideAmount,
        EffectiveAmount   = eb.OverrideAmount ?? b?.Amount ?? 0,
        IsActive          = eb.IsActive
    };
}
