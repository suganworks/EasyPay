using System.ComponentModel.DataAnnotations;

namespace EasyPay.Core.DTOs.Leave;

public class CreateLeaveRequestDto
{
    [Required] public int LeaveTypeId { get; set; }
    [Required] public DateOnly FromDate { get; set; }
    [Required] public DateOnly ToDate { get; set; }
    [StringLength(1000)] public string? Reason { get; set; }
    public bool IsHalfDay { get; set; }
    [StringLength(2)] public string? HalfDayType { get; set; }
}

public class ApproveLeaveDto
{
    [Required, StringLength(20)] public string Action { get; set; } = string.Empty;
    [StringLength(500)] public string? RejectionReason { get; set; }
}

public class LeaveRequestResponseDto
{
    public int LeaveRequestId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveCode { get; set; } = string.Empty;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public int TotalDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsHalfDay { get; set; }
    public string? HalfDayType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LeaveBalanceDto
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveCode { get; set; } = string.Empty;
    public int MaxDaysPerYear { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
    public bool IsPaid { get; set; }
}

public class LeaveCarryForwardDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<LeaveCarryForwardItemDto> CarryForwardItems { get; set; } = new();
}

public class LeaveCarryForwardItemDto
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int EligibleDays { get; set; }
    public int CarriedForwardDays { get; set; }
    public int LapsedDays { get; set; }
}
