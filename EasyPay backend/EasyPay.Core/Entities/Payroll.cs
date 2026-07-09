namespace EasyPay.Core.Entities;

public class Payroll : BaseEntity
{
    public int PayrollId { get; set; }
    public int EmployeeId { get; set; }
    public int PolicyId { get; set; }
    public int SalaryStructureId { get; set; }
    public DateOnly PayPeriodStart { get; set; }
    public DateOnly PayPeriodEnd { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal OvertimeHours { get; set; }

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal ConveyanceAllow { get; set; }
    public decimal MedicalAllow { get; set; }
    public decimal SpecialAllow { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal GrossEarnings { get; set; }

    // Deductions
    public decimal PfEmployee { get; set; }
    public decimal PfEmployer { get; set; }
    public decimal EsiEmployee { get; set; }
    public decimal EsiEmployer { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    public string Status { get; set; } = "Draft";
    public int? ProcessedById { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public Employee Employee { get; set; } = null!;
    public PayrollPolicy Policy { get; set; } = null!;
    public SalaryStructure SalaryStructure { get; set; } = null!;
}
