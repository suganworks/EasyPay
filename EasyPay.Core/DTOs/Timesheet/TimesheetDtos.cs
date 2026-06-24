using System.ComponentModel.DataAnnotations;

namespace EasyPay.Core.DTOs.Timesheet;

public class CreateTimesheetDto
{
    [Required] public DateOnly WorkDate { get; set; }
    public TimeOnly? CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    [Range(0, 24)] public decimal HoursWorked { get; set; }
    [Range(0, 24)] public decimal OvertimeHours { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
}

public class UpdateTimesheetDto
{
    public TimeOnly? CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    [Range(0, 24)] public decimal HoursWorked { get; set; }
    [Range(0, 24)] public decimal OvertimeHours { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
}

public class ApproveTimesheetDto
{
    [Required, StringLength(20)] public string Action { get; set; } = string.Empty;
}

public class TimesheetResponseDto
{
    public int TimesheetId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateOnly WorkDate { get; set; }
    public TimeOnly? CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TimesheetMonthlySummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public int PendingTimesheets { get; set; }
    public int ApprovedTimesheets { get; set; }
}

public class BulkApproveTimesheetDto
{
    [Required]
    public List<int> TimesheetIds { get; set; } = new();
}
