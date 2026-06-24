namespace EasyPay.Core.Entities;

public class PayrollPolicy : BaseEntity
{
    public int PolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string PayFrequency { get; set; } = "Monthly";
    public decimal OvertimeRate { get; set; } = 1.5m;
    public decimal PfEmployeeRate { get; set; } = 12.0m;
    public decimal PfEmployerRate { get; set; } = 12.0m;
    public decimal EsiEmployeeRate { get; set; } = 0.75m;
    public decimal EsiEmployerRate { get; set; } = 3.25m;
    public decimal ProfessionalTax { get; set; } = 200.0m;
    public decimal GratuityRate { get; set; } = 4.81m;
    public int WorkingDaysMonth { get; set; } = 26;
    public decimal WorkingHoursDay { get; set; } = 8.0m;
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public ICollection<SalaryStructure> SalaryStructures { get; set; } = new List<SalaryStructure>();
    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}
