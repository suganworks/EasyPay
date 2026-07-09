using System.ComponentModel.DataAnnotations;

namespace EasyPay.Core.DTOs.Payroll;

public class CreateSalaryStructureDto
{
    [Required] public int EmployeeId { get; set; }
    [Required] public int PolicyId { get; set; }
    [Required] public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    [Required, Range(0, double.MaxValue)] public decimal BasicSalary { get; set; }
    [Range(0, double.MaxValue)] public decimal HRA { get; set; }
    [Range(0, double.MaxValue)] public decimal ConveyanceAllowance { get; set; }
    [Range(0, double.MaxValue)] public decimal MedicalAllowance { get; set; }
    [Range(0, double.MaxValue)] public decimal SpecialAllowance { get; set; }
    [Range(0, double.MaxValue)] public decimal LTA { get; set; }
    [Range(0, double.MaxValue)] public decimal OtherAllowances { get; set; }
}

public class SalaryStructureResponseDto
{
    public int SalaryStructureId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int PolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal LTA { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    public bool IsActive { get; set; }
}

public class ProcessPayrollDto
{
    [Required] public int EmployeeId { get; set; }
    [Required] public DateOnly PayPeriodStart { get; set; }
    [Required] public DateOnly PayPeriodEnd { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal OtherDeductions { get; set; }
    public string? Remarks { get; set; }
}

public class BulkProcessPayrollDto
{
    [Required] public DateOnly PayPeriodStart { get; set; }
    [Required] public DateOnly PayPeriodEnd { get; set; }
    public List<int>? EmployeeIds { get; set; }
    public string? Remarks { get; set; }
}

public class PayrollResponseDto
{
    public int PayrollId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly PayPeriodStart { get; set; }
    public DateOnly PayPeriodEnd { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal ConveyanceAllow { get; set; }
    public decimal MedicalAllow { get; set; }
    public decimal SpecialAllow { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal PfEmployee { get; set; }
    public decimal PfEmployer { get; set; }
    public decimal EsiEmployee { get; set; }
    public decimal EsiEmployer { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProcessedByName { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Remarks { get; set; }
}

public class PayrollSummaryDto
{
    public int TotalEmployees { get; set; }
    public decimal TotalGrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetSalary { get; set; }
    public string PayPeriod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CreatePayrollPolicyDto
{
    [Required, StringLength(100)] public string PolicyName { get; set; } = string.Empty;
    [Required] public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    [Required, StringLength(20)] public string PayFrequency { get; set; } = "Monthly";
    public decimal OvertimeRate { get; set; } = 1.5m;
    public decimal PfEmployeeRate { get; set; } = 12.0m;
    public decimal PfEmployerRate { get; set; } = 12.0m;
    public decimal EsiEmployeeRate { get; set; } = 0.75m;
    public decimal EsiEmployerRate { get; set; } = 3.25m;
    public decimal ProfessionalTax { get; set; } = 200.0m;
    public decimal GratuityRate { get; set; } = 4.81m;
    public int WorkingDaysMonth { get; set; } = 26;
    public decimal WorkingHoursDay { get; set; } = 8.0m;
    [StringLength(500)] public string? Description { get; set; }
}

public class PayrollPolicyResponseDto
{
    public int PolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string PayFrequency { get; set; } = string.Empty;
    public decimal OvertimeRate { get; set; }
    public decimal PfEmployeeRate { get; set; }
    public decimal PfEmployerRate { get; set; }
    public decimal EsiEmployeeRate { get; set; }
    public decimal EsiEmployerRate { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal GratuityRate { get; set; }
    public int WorkingDaysMonth { get; set; }
    public decimal WorkingHoursDay { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

public class MarkAsPaidDto
{
    [Required]
    public DateOnly PaymentDate { get; set; }
}
