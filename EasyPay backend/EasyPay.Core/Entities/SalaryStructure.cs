namespace EasyPay.Core.Entities;

public class SalaryStructure : BaseEntity
{
    public int SalaryStructureId { get; set; }
    public int EmployeeId { get; set; }
    public int PolicyId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal LTA { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary => BasicSalary + HRA + ConveyanceAllowance + MedicalAllowance
                                  + SpecialAllowance + LTA + OtherAllowances;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Employee Employee { get; set; } = null!;
    public PayrollPolicy Policy { get; set; } = null!;
    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}
